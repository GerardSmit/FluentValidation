namespace FluentValidation.Internal;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Validators;

internal class AbstractValidationRule<T> : IValidationRule<T> {
	private readonly IValidationRule<T> _rule;

	public AbstractValidationRule(IValidationRule<T> rule) {
		_rule = rule;
	}

	public CascadeMode CascadeMode {
		get => _rule.CascadeMode;
		set => _rule.CascadeMode = value;
	}

	public IEnumerable<IRuleComponent> Components => _rule.Components;

	public string[] RuleSets {
		get => _rule.RuleSets;
		set => _rule.RuleSets = value;
	}

	public string GetDisplayName(IValidationContext context) {
		return _rule.GetDisplayName(context);
	}

	public void SetDisplayName(string name) {
		_rule.SetDisplayName(name);
	}

	public string PropertyName {
		get => _rule.PropertyName;
		set => _rule.PropertyName = value;
	}

	public MemberInfo Member => _rule.Member;

	public Type TypeToValidate => _rule.TypeToValidate;

	public bool HasCondition => _rule.HasCondition;

	public bool HasAsyncCondition => _rule.HasAsyncCondition;

	public LambdaExpression Expression => _rule.Expression;

	public IEnumerable<IValidationRule> DependentRules => _rule.DependentRules;

	public void SetDisplayName(Func<ValidationContext<T>, string> factory) {
		_rule.SetDisplayName(factory);
	}

	public void ApplyCondition(Func<ValidationContext<T>, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators) {
		_rule.ApplyCondition(predicate, applyConditionTo);
	}

	public void ApplyAsyncCondition(Func<ValidationContext<T>, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators) {
		_rule.ApplyAsyncCondition(predicate, applyConditionTo);
	}

	public void ApplySharedCondition(Func<ValidationContext<T>, bool> condition) {
		_rule.ApplySharedCondition(condition);
	}

	public void ApplySharedAsyncCondition(Func<ValidationContext<T>, CancellationToken, Task<bool>> condition) {
		_rule.ApplySharedAsyncCondition(condition);
	}

	public object GetPropertyValue(T instance) {
		return _rule.GetPropertyValue(instance);
	}

	public bool TryGetPropertyValue<TProp>(T instance, out TProp value) {
		return _rule.TryGetPropertyValue(instance, out value);
	}

	public ValueTask ValidateAsync(ValidationContext<T> context, CancellationToken cancellation) {
		return _rule.ValidateAsync(context, cancellation);
	}

	public void Validate(ValidationContext<T> context) {
		_rule.Validate(context);
	}

	public void AddDependentRules(IEnumerable<IValidationRule<T>> rules) {
		_rule.AddDependentRules(rules);
	}
}

internal class SelectValidationRule<T, TOld, TNew> : AbstractValidationRule<T>, IValidationRule<T, TNew> {
	 private readonly IValidationRule<T, TOld> _rule;
	 private readonly Func<T, TOld, TNew> _toNew;
	 private readonly Func<T, TNew, TOld> _toOld;

	 public SelectValidationRule(IValidationRule<T, TOld> rule, Func<T, TOld, TNew> toNew, Func<T, TNew, TOld> toOld)
		: base(rule) {
		 _rule = rule;
		 _toNew = toNew;
		 _toOld = toOld;
	 }

	 public void AddValidator(IPropertyValidator<T, TNew> validator) {
		 _rule.AddValidator(new SelectValidatorAdaptor<T, TOld, TNew>(_toNew, validator));
	 }

	 public void AddAsyncValidator(IAsyncPropertyValidator<T, TNew> asyncValidator, IPropertyValidator<T, TNew> fallback = null) {
		 _rule.AddAsyncValidator(
			 new AsyncSelectValidatorAdaptor<T, TOld, TNew>(_toNew, asyncValidator),
			 fallback is null ? null : new SelectValidatorAdaptor<T, TOld, TNew>(_toNew, fallback)
			);
	 }

	 public IRuleComponent<T, TNew> Current => null; // Not supported

	 public Func<IMessageBuilderContext<T, TNew>, string> MessageBuilder {
		 set {
			 _rule.MessageBuilder = context => {
				 var newValue = _toNew(context.InstanceToValidate, context.PropertyValue);
				 var component = new SelectRuleComponent<T, TOld, TNew>(_rule.Current, _toNew, _toOld);
				 var newContext = new MessageBuilderContext<T, TNew>(context.ParentContext, newValue, component);

				 return value(newContext);
			 };
		 }
	 }
}

file class SelectRuleComponent<T, TOld, TNew> : IRuleComponentInternal<T, TNew> {
	private readonly IRuleComponent<T, TOld> _component;
	private readonly Func<T, TOld, TNew> _select;
	private readonly Func<T, TNew, TOld> _toOld;

	public SelectRuleComponent(IRuleComponent<T, TOld> component, Func<T, TOld, TNew> select, Func<T, TNew, TOld> toOld) {
		_component = component;
		_select = select;
		_toOld = toOld;
	}

	public Type BaseType => _component.BaseType;

	public Type PropertyType => _component.PropertyType;

	public bool HasCondition => _component.HasCondition;

	public bool HasAsyncCondition => _component.HasAsyncCondition;

	public IPropertyValidator Validator => _component.Validator;

	public string GetUnformattedErrorMessage() {
		return _component.GetUnformattedErrorMessage();
	}

	string IRuleComponent<T, TNew>.ErrorCode {
		get => _component.ErrorCode;
		set => _component.ErrorCode = value;
	}

	public Func<ValidationContext<T>, TNew, object> CustomStateProvider {
		set {
			_component.CustomStateProvider = (context, oldValue) => value(context, _select(context.InstanceToValidate, oldValue));
		}
	}

	public Func<ValidationContext<T>, TNew, Severity> SeverityProvider {
		set {
			_component.SeverityProvider = (context, oldValue) => value(context, _select(context.InstanceToValidate, oldValue));
		}
	}

	public void ApplyCondition(Func<ValidationContext<T>, bool> condition) {
		_component.ApplyCondition(condition);
	}

	public void ApplyAsyncCondition(Func<ValidationContext<T>, CancellationToken, Task<bool>> condition) {
		_component.ApplyAsyncCondition(condition);
	}

	public void SetErrorMessage(Func<ValidationContext<T>, TNew, string> errorFactory) {
		_component.SetErrorMessage((context, oldValue) => errorFactory(context, _select(context.InstanceToValidate, oldValue)));
	}

	public void SetErrorMessage(string errorMessage) {
		_component.SetErrorMessage(errorMessage);
	}

	public bool InvokeCondition(ValidationContext<T> context) {
		return _component.InvokeCondition(context);
	}

	public Task<bool> InvokeAsyncCondition(ValidationContext<T> context, CancellationToken token = default) {
		return _component.InvokeAsyncCondition(context, token);
	}

	public string GetErrorMessage(ValidationContext<T> innerContext, TNew value) {
		if (_toOld is null) {
			throw new InvalidOperationException("Cannot get error message without a conversion function to the old type.");
		}

		var oldValue = _toOld(innerContext.InstanceToValidate, value);

		if (_component is IRuleComponentInternal<T, TOld> errorMessageProvider) {
			return errorMessageProvider.GetErrorMessage(innerContext, oldValue);
		}

		return _component.GetUnformattedErrorMessage();
	}

	public string ErrorCode => ((IRuleComponent)_component).ErrorCode;
}

file class AsyncSelectValidatorAdaptor<T, TOld, TNew> : AsyncPropertyValidator<T, TOld> {
	private readonly IAsyncPropertyValidator<T, TNew> _validator;
	private readonly Func<T, TOld, TNew> _select;

	public AsyncSelectValidatorAdaptor(Func<T, TOld, TNew> select, IAsyncPropertyValidator<T, TNew> validator) {
		_select = select;
		_validator = validator;
	}

	public override string Name => _validator.Name;

	public override Task<bool> IsValidAsync(ValidationContext<T> context, TOld value, CancellationToken cancellation) {
		var newValue = _select(context.InstanceToValidate, value);

		return _validator.IsValidAsync(context, newValue, cancellation);
	}
}

internal class SelectValidatorAdaptor<T, TOld, TNew> : PropertyValidator<T, TOld> {
	private readonly IPropertyValidator<T, TNew> _validator;
	private readonly Func<T, TOld, TNew> _select;

	public SelectValidatorAdaptor(Func<T, TOld, TNew> select, IPropertyValidator<T, TNew> validator) {
		_select = select;
		_validator = validator;
	}

	public override string Name => _validator.Name;

	public override bool IsValid(ValidationContext<T> context, TOld value) {
		var newValue = _select(context.InstanceToValidate, value);
		return _validator.IsValid(context, newValue);
	}
}
