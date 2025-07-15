namespace FluentValidation.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using Validators;

internal class ConditionalValidationRule<T, TProperty> : AbstractValidationRule<T>, IValidationRule<T, TProperty> {
	private readonly IValidationRule<T, TProperty> _rule;
	private readonly Func<T, TProperty, bool> _condition;

	public ConditionalValidationRule(IValidationRule<T, TProperty> rule, Func<T, TProperty, bool> condition) : base(rule) {
		_rule = rule;
		_condition = condition;
	}

	public void AddValidator(IPropertyValidator<T, TProperty> validator) {
		_rule.AddValidator(new ConditionalValidatorAdaptor<T, TProperty>(_condition, validator));
	}

	public void AddAsyncValidator(IAsyncPropertyValidator<T, TProperty> asyncValidator, IPropertyValidator<T, TProperty> fallback = null) {
		_rule.AddAsyncValidator(
			new AsyncConditionalValidatorAdaptor<T, TProperty>(_condition, asyncValidator),
			fallback is null ? null : new ConditionalValidatorAdaptor<T, TProperty>(_condition, fallback)
		);
	}

	public IRuleComponent<T, TProperty> Current => _rule.Current;

	public Func<IMessageBuilderContext<T, TProperty>, string> MessageBuilder {
		set => _rule.MessageBuilder = value;
	}
}

file class AsyncConditionalValidatorAdaptor<T, TProperty> : AsyncPropertyValidator<T, TProperty> {
	private readonly IAsyncPropertyValidator<T, TProperty> _validator;
	private readonly Func<T, TProperty, bool> _condition;

	public AsyncConditionalValidatorAdaptor(Func<T, TProperty, bool> condition, IAsyncPropertyValidator<T, TProperty> validator) {
		_condition = condition;
		_validator = validator;
	}

	public override string Name => _validator.Name;

	public override Task<bool> IsValidAsync(ValidationContext<T> context, TProperty value, CancellationToken cancellation) {
		if (!_condition(context.InstanceToValidate, value)) {
			return Task.FromResult(true); // Skip validation if condition is not met
		}

		return _validator.IsValidAsync(context, value, cancellation);
	}
}

internal class ConditionalValidatorAdaptor<T, TProperty> : PropertyValidator<T, TProperty> {
	private readonly IPropertyValidator<T, TProperty> _validator;
	private readonly Func<T, TProperty, bool> _condition;

	public ConditionalValidatorAdaptor(Func<T, TProperty, bool> condition, IPropertyValidator<T, TProperty> validator) {
		_condition = condition;
		_validator = validator;
	}

	public override string Name => _validator.Name;

	public override bool IsValid(ValidationContext<T> context, TProperty value) {
		if (!_condition(context.InstanceToValidate, value)) {
			return true; // Skip validation if condition is not met
		}

		return _validator.IsValid(context, value);
	}
}

