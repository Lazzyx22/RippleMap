using RippleMap.Tests.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace RippleMap.Tests;

public class ResourceIdRulesTests
{
    [Theory]
    [InlineData("checkout", true)]
    [InlineData("orders-db", true)]
    [InlineData("service2", false)]
    [InlineData("checkout", false)]
    [InlineData("2checkout", false)]
    [InlineData("orders-db", false)]
    [InlineData("",false)]
    [InlineData(null,false)]
    public void IsValid_ReturnsExpectedResult(string? id, bool expected)
    {
        // Act
        bool result = ResourceIdRules.IsValid(id);
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_Accepts64Characters()
    {
        // Arrange
        string id = new string('a', 64);
        // Act
        bool actual = ResourceIdRules.IsValid(id);
        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValid_Rejects65Characters()
    {
        // Arrange
        string id = new string('a', 65);
        
        Assert.False(ResourceIdRules.IsValid(id));
    }
}