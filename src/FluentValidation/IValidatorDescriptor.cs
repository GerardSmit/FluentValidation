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

namespace FluentValidation;

using System.Collections.Generic;
using System.Linq;
using Internal;
using Validators;

/// <summary>
/// Provides metadata about a validator.
/// </summary>
public interface IValidatorDescriptor {

	/// <summary>
	/// All rules defined in the validator.
	/// </summary>
	IEnumerable<IValidationRule> Rules { get; }

	/// <summary>
	/// Gets the name display name for a property.
	/// </summary>
	string GetName(string property);

	/// <summary>
	/// Gets a collection of validators grouped by property.
	/// </summary>
	ILookup<string, (IPropertyValidator Validator, IRuleComponent Options)> GetMembersWithValidators();

	/// <summary>
	/// Gets validators for a particular property.
	/// </summary>
	IEnumerable<(IPropertyValidator Validator, IRuleComponent Options)> GetValidatorsForMember(string name);

	/// <summary>
	/// Gets rules for a property.
	/// </summary>
	IEnumerable<IValidationRule> GetRulesForMember(string name);

	/// <summary>
	/// Creates a new validation context for the specified instance.
	/// </summary>
	IValidationContext CreateContext(object instance);
}
