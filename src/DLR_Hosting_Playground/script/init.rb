#!/usr/bin/ruby

#CAUTION: don't create files with the visual studio for ruby
#wrong encoding! use notepad or something else
require "DLR_hosting"

puts "From Ruby:"
#scriptScopeVariable won't be avaiable from the Hosting application's ScriptScope,
#like in IronPython
scriptScopeVariable = "I'm a ruby"
puts scriptScopeVariable + " in Ruby"
# instance names are not allowed to be Uppercase, exception is thrown
sharpClass = DLR_hosting::CSharpClass.new("creating from Ruby")
sharpClass.Var = "Hello Ruby!"
puts sharpClass.Var

def scriptScopeVariable ()
	scriptScopeVariable
end
