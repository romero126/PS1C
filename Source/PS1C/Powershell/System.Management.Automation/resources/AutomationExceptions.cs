using System;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public static class AutomationExceptions
    {

       internal static string Argument                                                                                             =   "Cannot process argument because the value of argument {0} is not valid. Change the value of the {0} argument and run the operation again."; 
       internal static string InvalidScopeIdArgument                                                                               =   "Cannot process argument because the value of parameter {0} is not valid. Valid values are \"jGlobal\"j, \"jLocal\"j, or \"jScript\"j, or a number relative to the current scope (0 through the number of scopes where 0 is the current scope and 1 is its parent). Change the value of the {0} parameter and run the operation again."; 
       internal static string ArgumentNull                                                                                         =   "Cannot process argument because the value of argument {0} is null. Change the value of argument {0} to a non-null value."; 
       internal static string ArgumentOutOfRange                                                                                   =   "Cannot process argument because the value of argument {0} is out of range. Change argument {0} to a value that is within range."; 
       internal static string InvalidOperation                                                                                     =   "Cannot perform operation because operation {0} is not valid. Remove operation {0}, or investigate why it is not valid."; 
       internal static string NotImplemented                                                                                       =   "Cannot perform operation because operation {0} is not implemented."; 
       internal static string NotSupported                                                                                         =   "Cannot perform operation because operation {0} is not supported."; 
       internal static string ObjectDisposed                                                                                       =   "Cannot perform operation because object {0} has already been disposed."; 
       internal static string ScriptBlockInvokeOnOneClauseOnly                                                                     =   "The script block cannot be invoked because it contains more than one clause. The Invoke() method can only be used on script blocks that contain a single clause."; 
       internal static string CanConvertOneClauseOnly                                                                              =   "The script block cannot be converted because it contains more than one clause. Expressions or control structures are not permitted. Verify that the script block contains exactly one pipeline or command."; 
       internal static string CantConvertEmptyPipeline                                                                             =   "An empty script block cannot be converted. Verify that the script block contains exactly one pipeline or command."; 
       internal static string CanOnlyConvertOnePipeline                                                                            =   "Only a script block that contains exactly one pipeline or command can be converted. Expressions or control structures are not permitted. Verify that the script block contains exactly one pipeline or command."; 
       internal static string CantConvertScriptBlockWithTrap                                                                       =   "A script block that contains a top-level trap statement cannot be converted."; 
       internal static string CantConvertWithUndeclaredVariables                                                                   =   "Cannot generate a PowerShell object for a ScriptBlock dereferencing variables undeclared in the param(...) block.  Name of undeclared variable: {0}."; 
       internal static string CantConvertWithNonConstantExpression                                                                 =   "Cannot generate a PowerShell object for a ScriptBlock evaluating non-constant expressions. Non-constant expression: {0}."; 
       internal static string CantConvertWithDynamicExpression                                                                     =   "Cannot generate a PowerShell object for a ScriptBlock evaluating dynamic expressions. Dynamic expression: {0}."; 
       internal static string CantConvertWithScriptBlocks                                                                          =   "Cannot generate a PowerShell object for a ScriptBlock that tries to pass other script blocks inside argument values."; 
       internal static string CantConvertWithCommandInvocations                                                                    =   "Cannot generate a PowerShell object for a ScriptBlock which invokes pipelines, commands or functions to evaluate arguments of the main pipeline."; 
       internal static string CantConvertWithDotSourcing                                                                           =   "Cannot generate a PowerShell object for a ScriptBlock that uses dot sourcing."; 
       internal static string CantConvertWithScriptBlockInvocation                                                                 =   "Cannot generate a PowerShell object for a ScriptBlock that invokes other script blocks."; 
       internal static string CanConvertOneOutputErrorRedir                                                                        =   "The script block cannot be converted to a PowerShell object because it contains forbidden redirection operators."; 
       internal static string CantConvertScriptBlockWithNoContext                                                                  =   "Cannot generate a PowerShell object for a ScriptBlock that does not have an associated operation context."; 
       internal static string HaltCommandException                                                                                 =   "The command was stopped by the user."; 
       internal static string DynamicParametersWrongType                                                                           =   "Object {0} is the wrong type to return from the dynamicparam block. The dynamicparam block must return either $null, or an object with type [System.Management.Automation.RuntimeDefinedParameterDictionary]."; 
       internal static string CantConvertScriptBlockToOpenGenericType                                                              =   "The script block cannot be converted to an open generic type. Define an appropriate closed generic type, and then retry."; 
       internal static string CantConvertPipelineStartsWithExpression                                                              =   "Cannot generate a PowerShell object for a ScriptBlock that starts a pipeline with an expression."; 
       internal static string UsingVariableIsUndefined                                                                             =   "The value of the using variable '$using:{0}' cannot be retrieved because it has not been set in the local session."; 
       internal static string CantGetUsingExpressionValueWithSpecifiedVariableDictionary                                           =   "Cannot get the value of the Using expression '{0}' in the specified variable dictionary. When creating a PowerShell instance from a script block, the Using expression cannot contain an indexing operation or member-accessing operation.";

    }
}

