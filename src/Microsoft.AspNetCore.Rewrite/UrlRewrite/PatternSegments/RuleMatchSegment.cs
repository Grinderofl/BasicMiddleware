﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite.PatternSegments
{
    public class RuleMatchSegment : PatternSegment
    {
        public int Index { get; set; }

        public RuleMatchSegment(int index)
        {
            Index = index;
        }

        public override string Evaluate(HttpContext context, Match ruleMatch, Match condMatch)
        {
            return ruleMatch?.Groups[Index]?.Value;
        }
    }
}
