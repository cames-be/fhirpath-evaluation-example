---
title: Firely FhirPath Examples
google-site-verification: ZpdDtXaTbbaAaUeJ7YVw21oUeZ-SIs-uQZOnZos1nHM
---
<meta name="google-site-verification" content="L0emuyFDx7zeUw2KdXoTnnpL6NZc0ELR439snORXwL4" />

# Firely FhirPath Examples

Example code for evaluating FhirPath expressions using Firely.  Self hosts endpoints for REST calls to locate external Fhir resources with sample json files.  Examples include:

* Basic expression compilation and evaluation
* Use of `resolve()` FhirPath function to resolve referenced resources
  * Contained resources
    * Requirement of `ScopedNode` when using `resolve()` to locate contained resources
  * External resources
    * Use of the `ElementResolver` property of the `FhirEvaluationContext` class to resolve external resources
* Standard and extended functions in the `SymbolTable`
  * Standard functions (default in `FhirPathCompiler`) - [Link](https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.FhirPath/FhirPath/Expressions/SymbolTableInit.cs#L24)
  * ExtensionFunctions - [Link](https://github.com/FirelyTeam/firely-net-common/blob/3d981d0679a4431b9b50dc55578b50fc9d7ae426/src/Hl7.Fhir.Support.Poco/FhirPath/ElementNavFhirExtensions.cs#L24)
    * `hasValue`
    * `resolve`
    * `memberOf`
    * `htmlChecks`
* Use of `FhirClient` to query a REST endpoint for resources
* Custom FhirPath function used in an expression
* Custom `EvaluationContext` for passing custom instance into a custom function 
