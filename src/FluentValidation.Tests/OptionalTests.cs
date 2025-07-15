namespace FluentValidation.Tests;

using System;
using System.Linq.Expressions;
using Xunit;

public class OptionalTests {
	public OptionalTests() {
		CultureScope.SetDefaultCulture();
	}

	[Fact]
	public void When_there_is_a_value_then_the_validator_should_pass() {
		var validator = new InlineValidator<UpdatePerson> {
			v => v.RuleForOptional(x => x.Name).NotEmpty()
		};

		var result = validator.Validate(new UpdatePerson { Name = "Foo" });
		result.IsValid.ShouldBeTrue();
	}

	[Fact]
	public void When_there_is_no_value_then_the_validator_should_pass() {
		var validator = new InlineValidator<UpdatePerson> {
			v => v.RuleForOptional(x => x.Name).NotEmpty()
		};

		var result = validator.Validate(new UpdatePerson { Name = default });
		result.IsValid.ShouldBeTrue();
	}

	[Fact]
	public void When_there_is_a_empty_value_then_the_validator_should_fail() {
		var validator = new InlineValidator<UpdatePerson> {
			v => v.RuleForOptional(x => x.Name).NotEmpty()
		};

		var result = validator.Validate(new UpdatePerson { Name = "" });
		result.IsValid.ShouldBeFalse();
	}
}

internal static class OptionalExtensions {
	public static IRuleBuilder<T, TProperty> RuleForOptional<T, TProperty>(this AbstractValidator<T> validator, Expression<Func<T, Optional<TProperty>>> expression) {
		return validator.RuleFor(expression)
			.Where(x => x.HasValue)
			.Select(
				(_, v) => v.Value,
				(_, v) => new Optional<TProperty>(v)
			);
	}
}
