﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.UrlRewrite;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class FileParserTests
    {
        [Fact]
        public void RuleParse_ParseTypicalRule()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(new List<Condition>(),
                IMurl: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                UAactionType: ActionType.Rewrite,
                UApattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = FileParser.Parse(new StringReader(xml));

            // assert
            Assert.True(CheckUrlRewriteRuleEquality(res, expected));
        }

        [Fact]
        public void RuleParse_ParseSingleRuleWithSingleCondition()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var condList = new List<Condition>();
            condList.Add(new Condition
            {
                Input = InputParser.ParseInputString("{HTTPS}"),
                MatchPattern = new Regex("^OFF$")
            });

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(condList,
                IMurl: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                UAactionType: ActionType.Rewrite,
                UApattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = FileParser.Parse(new StringReader(xml));
            
            // assert
            Assert.True(CheckUrlRewriteRuleEquality(expected, res));
        }


        [Fact]
        public void RuleParse_ParseMultipleRules()
        {
            // arrange
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <conditions>  
                                        <add input=""{HTTPS}"" pattern=""^OFF$"" />  
                                    </conditions>  
                                    <action type=""Redirect"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";

            var condList = new List<Condition>();
            condList.Add(new Condition
            {
                Input = InputParser.ParseInputString("{HTTPS}"),
                MatchPattern = new Regex("^OFF$")
            });

            var expected = new List<UrlRewriteRule>();
            expected.Add(CreateTestRule(condList,
                IMurl: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                UAactionType: ActionType.Rewrite,
                UApattern: "article.aspx?id={R:1}&amp;title={R:2}"));
            expected.Add(CreateTestRule(condList,
                IMurl: "^article/([0-9]+)/([_0-9a-z-]+)",
                name: "Rewrite to article.aspx",
                UAactionType: ActionType.Redirect,
                UApattern: "article.aspx?id={R:1}&amp;title={R:2}"));

            // act
            var res = FileParser.Parse(new StringReader(xml));

            // assert
            Assert.True(CheckUrlRewriteRuleEquality(expected, res));
        }


        // Creates a rule with appropriate default values of the url rewrite rule.
        private UrlRewriteRule CreateTestRule(List<Condition> conditions,
            LogicalGrouping condGrouping = LogicalGrouping.MatchAll,
            bool condTracking = false,
            string name = "",
            bool enabled = true,
            PatternSyntax patternSyntax = PatternSyntax.ECMAScript,
            bool stopProcessing = false,
            string IMurl = "",
            bool IMignoreCase = true,
            bool IMnegate = false,
            ActionType UAactionType = ActionType.None,
            string UApattern = "",
            bool UAappendQueryString = false,
            bool UAlogRewrittenUrl = false,
            RedirectType UAredirectType = RedirectType.Permanent
            )
        {
            return new UrlRewriteRule
            {
                Action = new UrlAction
                {
                    Url = InputParser.ParseInputString(UApattern),
                    Type = UAactionType,
                    AppendQueryString = UAappendQueryString,
                    LogRewrittenUrl = UAlogRewrittenUrl,
                    RedirectType = UAredirectType
                },
                Name = name,
                Enabled = enabled,
                StopProcessing = stopProcessing,
                PatternSyntax = patternSyntax,
                Match = new InitialMatch
                {
                    Url = new Regex(IMurl),
                    IgnoreCase = IMignoreCase,
                    Negate = IMnegate
                },
                Conditions = new Conditions
                {
                    ConditionList = conditions,
                    MatchType = condGrouping,
                    TrackingAllCaptures = condTracking
                }
                
            };
        }

        private bool CheckUrlRewriteRuleEquality(List<UrlRewriteRule> expected, List<UrlRewriteRule> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }
            for (var i = 0; i < expected.Count; i++)
            {
                var r1 = expected[i];
                var r2 = actual[i];

                if (r1.Name != r2.Name
                    || r1.Enabled != r2.Enabled
                    || r1.StopProcessing != r2.StopProcessing
                    || r1.PatternSyntax != r2.PatternSyntax)
                {
                    return false;
                }

                if (r1.Match.IgnoreCase != r2.Match.IgnoreCase
                    || r1.Match.Negate != r2.Match.Negate)
                {
                    return false;
                }

                if (r1.Action.Type != r2.Action.Type
                    || r1.Action.AppendQueryString != r2.Action.AppendQueryString
                    || r1.Action.RedirectType != r2.Action.RedirectType
                    || r1.Action.LogRewrittenUrl != r2.Action.LogRewrittenUrl)
                {
                    return false;
                }

                // TODO conditions, url pattern, initial match regex
                if (r1.Conditions.MatchType != r2.Conditions.MatchType
                    || r1.Conditions.TrackingAllCaptures != r2.Conditions.TrackingAllCaptures)
                {
                    return false;
                }

                if (r1.Conditions.ConditionList.Count != r2.Conditions.ConditionList.Count)
                {
                    return false;
                }
                
                for (var j = 0; j < r1.Conditions.ConditionList.Count; j++)
                {
                    var c1 = r1.Conditions.ConditionList[j];
                    var c2 = r2.Conditions.ConditionList[j];
                    if (c1.IgnoreCase != c2.IgnoreCase
                        || c1.Negate != c2.Negate
                        || c1.MatchType != c2.MatchType)
                    {
                        return false;
                    }
                    if (c1.Input.PatternSegments.Count != c2.Input.PatternSegments.Count)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
