#!/usr/bin/python
import clr
clr.AddReference("DLR_hosting")
import DLR_hosting

scriptScopeVariable = "I'm a python"
sharpClass = DLR_hosting.CSharpClass("creating from Python")
print sharpClass.Var