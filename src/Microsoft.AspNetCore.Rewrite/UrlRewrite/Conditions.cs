﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Conditions
    {
        public List<Condition> ConditionList { get; set; } = new List<Condition>();
        public LogicalGrouping MatchType { get; set; } // default is MatchAll
        public bool TrackingAllCaptures { get; set; } 

        public MatchResults Evaluate(HttpContext context, MatchResults ruleMatch)
        {
            MatchResults prevCond = null;
            var success = false;
            foreach (var condition in ConditionList)
            {
                var res = condition.Match(condition.Input.Evaluate(context, ruleMatch, prevCond));
                // TODO consider refactor; this assumes there will only ever be those two values in the enum
                success = MatchType == LogicalGrouping.MatchAll ? success && res.Success : success || res.Success;
                prevCond = res;
            }
            return new MatchResults { Success = success, BackReference = prevCond.BackReference };
        }
    }
}
