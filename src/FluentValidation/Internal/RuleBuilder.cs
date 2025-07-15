#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FluentValidation.Internal;

using System;
using System.Collections.Generic;
using Validators;

/// <summary>
/// Builds a validation rule and constructs a validator.
/// </summary>
/// <typeparam name="T">Type of object being validated</typeparam>
/// <typeparam name="TProperty">Type of property being validated</typeparam>
internal class RuleBuilder<T, TProperty> : IRuleBuilderOptions<T, TProperty>, IRuleBuilderInitial<T, TProperty>, IRuleBuilderInitialCollection<T,TProperty>, IRuleBuilderOptionsConditions<T, TProperty> {

	/// <summary>
	/// The rule being created by this RuleBuilder.
	/// </summary>
	public IValidationRule<T, TProperty> Rule { get; }

	/// <summary>
	/// Parent validator
	/// </summary>
	public AbstractValidator<T> ParentValidator { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="RuleBuilder{T,TProperty}">RuleBuilder</see> class.
	/// </summary>
	public RuleBuilder(IValidationRule<T, TProperty> rule, AbstractValidator<T> parent) {
		Rule = rule;
		ParentValidator = parent;
	}

	public IRuleBuilderOptions<T, TProperty> SetValidator(IPropertyValidator<T, TProperty> validator) {
		if (validator == null) throw new ArgumentNullException(nameof(validator));
		AddValidator(validator);
		return this;
	}

	public IRuleBuilderOptions<T, TProperty> SetAsyncValidator(IAsyncPropertyValidator<T, TProperty> validator) {
		if (validator == null) throw new ArgumentNullException(nameof(validator));
		// See if the async validator supports synchronous execution too.
		IPropertyValidator<T, TProperty> fallback = validator as IPropertyValidator<T, TProperty>;
		AddAsyncValidator(validator, fallback);
		return this;
	}

	public IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator, params string[] ruleSets) {
		ArgumentNullException.ThrowIfNull(validator);
		var adaptor = new ChildValidatorAdaptor<T,TProperty>(validator, validator.GetType()) {
			RuleSets = ruleSets
		};
		// ChildValidatorAdaptor supports both sync and async execution.
		AddAsyncValidator(adaptor, adaptor);
		return this;
	}

	public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>(Func<T, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<TProperty> {
		ArgumentNullException.ThrowIfNull(validatorProvider);
		var adaptor = new ChildValidatorAdaptor<T,TProperty>((context, _) => validatorProvider(context.InstanceToValidate), typeof (TValidator)) {
			RuleSets = ruleSets
		};
		// ChildValidatorAdaptor supports both sync and async execution.
		AddAsyncValidator(adaptor, adaptor);
		return this;
	}

	public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>(Func<T, TProperty, TValidator> validatorProvider, params string[] ruleSets) where TValidator : IValidator<TProperty> {
		ArgumentNullException.ThrowIfNull(validatorProvider);
		var adaptor = new ChildValidatorAdaptor<T,TProperty>((context, val) => validatorProvider(context.InstanceToValidate, val), typeof (TValidator)) {
			RuleSets = ruleSets
		};
		// ChildValidatorAdaptor supports both sync and async execution.
		AddAsyncValidator(adaptor, adaptor);
		return this;
	}

	protected virtual void AddValidator(IPropertyValidator<T, TProperty> validator) {
		Rule.AddValidator(validator);
	}

	protected virtual void AddAsyncValidator(IAsyncPropertyValidator<T, TProperty> asyncValidator, IPropertyValidator<T, TProperty> fallback = null) {
		Rule.AddAsyncValidator(asyncValidator, fallback);
	}

	IRuleBuilderOptions<T, TProperty> IRuleBuilderOptions<T, TProperty>.DependentRules(Action action) {
		DependentRulesInternal(action);
		return this;
	}

	IRuleBuilderOptionsConditions<T, TProperty> IRuleBuilderOptionsConditions<T, TProperty>.DependentRules(Action action) {
		DependentRulesInternal(action);
		return this;
	}

	protected virtual void DependentRulesInternal(Action action) {
		var dependencyContainer = new List<IValidationRule<T>>();
		// Capture any rules added to the parent validator inside this delegate.
		using (ParentValidator.Rules.Capture(dependencyContainer.Add)) {
			action();
		}

		if (Rule.RuleSets != null && Rule.RuleSets.Length > 0) {
			foreach (var dependentRule in dependencyContainer) {
				if (dependentRule.RuleSets == null) {
					dependentRule.RuleSets = Rule.RuleSets;
				}
			}
		}

		Rule.AddDependentRules(dependencyContainer);
	}

	public virtual void AddComponent(RuleComponent<T,TProperty> component) {
		if (Rule.Components is not ICollection<RuleComponent<T,TProperty>> collection) {
			throw new InvalidOperationException("Cannot add a component to a rule that does not support components.");
		}

		collection.Add(component);
	}

}
