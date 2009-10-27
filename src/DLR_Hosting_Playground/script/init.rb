#!/usr/bin/ruby

#CAUTION: don't create files with the visual studio for ruby
#wrong encoding! use notepad or something else
require "DLR_hosting"

puts "From Ruby:"
scriptScopeVariable = "I'm a ruby"
puts scriptScopeVariable + " in Ruby"
# instance names are not allowed to be Uppercase, exception is thrown
sharpClass = DLR_hosting::CSharpClass.new("creating from Ruby")
sharpClass.Var = "Hello Ruby!"
puts sharpClass.Var