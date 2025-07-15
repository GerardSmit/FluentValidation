# GerardSmit.FluentValidation

This is a fork of the [FluentValidation](https://github.com/FluentValidation/FluentValidation) with changes that makes my life a little bit easier 🙂

1. Added basic LINQ support (`Select`, `Where`) for `IRuleBuilder<T, TProperty>` to filter and transform validation rules without having to make a `When`-clause for every rule.
  
   For example, I use `Optional<T>` type in my API responses, and I wanted to validate the properties of the `Optional<T>` type without constantly checking if the value is present.  
   Before:  
   ```csharp
   v.When(x => x.Name.HasValue, () => v.RuleFor(x => x.Name.Value).NotEmpty());
   ```

    After with a extension method:
    ```csharp
    v.RuleForOptional(x => x.Name).NotEmpty();

    // Extension
    internal static class OptionalExtensions
    {
      public static IRuleBuilder<T, TProperty> RuleForOptional<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, Optional<TProperty>>> expression
      )
      {
        return validator.RuleFor(expression)
          .Where(x => x.HasValue)
          .Select(
            (_, v) => v.Value,
            (_, v) => new Optional<TProperty>(v)
          );
      }
    }
    ```

2. Added non-generic interfaces, so I can get values I want without using reflection.

Below is the original README from the FluentValidation project, which contains more information about the library.

---

<p>
<img src="https://raw.githubusercontent.com/FluentValidation/FluentValidation/gh-pages/assets/images/logo/fluent-validation-logo.png" alt="FluentValidation" width="250px" />
</p>

[![Build Status](https://github.com/FluentValidation/FluentValidation/workflows/CI/badge.svg)](https://github.com/FluentValidation/FluentValidation/actions?query=workflow%3ACI) [![NuGet](https://img.shields.io/nuget/v/FluentValidation.svg)](https://nuget.org/packages/FluentValidation) [![Nuget](https://img.shields.io/nuget/dt/FluentValidation.svg)](https://nuget.org/packages/FluentValidation)

[Full Documentation](https://fluentvalidation.net)

A validation library for .NET that uses a fluent interface
and lambda expressions for building strongly-typed validation rules.

---
### Supporting the project
If you use FluentValidation in a commercial project, please sponsor the project financially. FluentValidation is developed and supported by [@JeremySkinner](https://github.com/JeremySkinner) for free in his spare time and financial sponsorship helps keep the project going. You can sponsor the project via either [GitHub sponsors](https://github.com/sponsors/JeremySkinner) or [OpenCollective](https://opencollective.com/FluentValidation).

---

### Get Started

FluentValidation can be installed using the Nuget package manager or the `dotnet` CLI.

```
dotnet add package FluentValidation
```

[Review our documentation](https://docs.fluentvalidation.net) for instructions on how to use the package.

---

### Example
```csharp
using FluentValidation;

public class CustomerValidator: AbstractValidator<Customer> {
  public CustomerValidator() {
    RuleFor(x => x.Surname).NotEmpty();
    RuleFor(x => x.Forename).NotEmpty().WithMessage("Please specify a first name");
    RuleFor(x => x.Discount).NotEqual(0).When(x => x.HasDiscount);
    RuleFor(x => x.Address).Length(20, 250);
    RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");
  }

  private bool BeAValidPostcode(string postcode) {
    // custom postcode validating logic goes here
  }
}

var customer = new Customer();
var validator = new CustomerValidator();

// Execute the validator
ValidationResult results = validator.Validate(customer);

// Inspect any validation failures.
bool success = results.IsValid;
List<ValidationFailure> failures = results.Errors;
```

### License, Copyright etc

FluentValidation has adopted the [Code of Conduct](https://github.com/FluentValidation/FluentValidation/blob/main/.github/CODE_OF_CONDUCT.md) defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

FluentValidation is copyright &copy; 2008-2022 .NET Foundation, [Jeremy Skinner](https://jeremyskinner.co.uk) and other contributors and is licensed under the [Apache2 license](https://github.com/JeremySkinner/FluentValidation/blob/master/License.txt).

### Sponsors

This project is sponsored by the following organisations whose support help keep this project going:

- [Microsoft](https://microsoft.com) for their financial contribution 
- [JetBrains](https://www.jetbrains.com/?from=FluentValidation) for providing licenses to their developer tools

This project is part of the [.NET Foundation](https://dotnetfoundation.org).
