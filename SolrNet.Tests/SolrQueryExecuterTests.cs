﻿#region license
// Copyright (c) 2007-2009 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Rhino.Mocks;
using SolrNet.Attributes;
using SolrNet.Commands.Parameters;
using SolrNet.Utils;

namespace SolrNet.Tests {
    [TestFixture]
    public class SolrQueryExecuterTests {
        public class TestDocument : ISolrDocument {
            [SolrUniqueKey]
            public int Id { get; set; }

            public string OtherField { get; set; }
        }

        [Test]
        public void Execute() {
            const string queryString = "id:123456";
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocument>>();
            var mockR = mocks.DynamicMock<ISolrQueryResults<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(() => {
                var q = new Dictionary<string, string>();
                q["q"] = queryString;
                Expect.Call(conn.Get("/select", q)).Repeat.Once().Return("");
                Expect.Call(parser.Parse(null)).IgnoreArguments().Repeat.Once().Return(mockR);
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                var r = queryExecuter.Execute(new SolrQuery(queryString), null);
            });
        }

        [Test]
        public void Sort() {
            const string queryString = "id:123456";
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocument>>();
            var mockR = mocks.DynamicMock<ISolrQueryResults<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(() => {
                var q = new Dictionary<string, string>();
                q["q"] = queryString;
                q["rows"] = SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString();
                q["sort"] = "id asc";
                Expect.Call(conn.Get("/select", q))
                    .Repeat.Once()
                    .Return("");
                Expect.Call(parser.Parse(null))
                    .IgnoreArguments()
                    .Repeat.Once()
                    .Return(mockR);
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                var r = queryExecuter.Execute(new SolrQuery(queryString), new QueryOptions {
                    OrderBy = new[] {new SortOrder("id")}
                });
            });
        }

        [Test]
        public void SortMultipleWithOrders() {
            const string queryString = "id:123456";
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocument>>();
            var mockR = mocks.DynamicMock<ISolrQueryResults<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(() => {
                var q = new Dictionary<string, string>();
                q["q"] = queryString;
                q["rows"] = SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString();
                q["sort"] = "id asc,name desc";
                Expect.Call(conn.Get("/select", q)).Repeat.Once().Return("");
                Expect.Call(parser.Parse(null)).IgnoreArguments().Repeat.Once().Return(mockR);
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                var r = queryExecuter.Execute(new SolrQuery(queryString), new QueryOptions {
                    OrderBy = new[] {
                        new SortOrder("id", Order.ASC),
                        new SortOrder("name", Order.DESC)
                    }
                });
            });
        }

        [Test]
        public void ResultFields() {
            const string queryString = "id:123456";
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.CreateMock<ISolrQueryResultParser<TestDocument>>();
            var mockR = mocks.DynamicMock<ISolrQueryResults<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(delegate {
                var q = new Dictionary<string, string>();
                q["q"] = queryString;
                q["rows"] = SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString();
                q["fl"] = "id,name";
                Expect.Call(conn.Get("/select", q)).Repeat.Once().Return("");
                Expect.Call(parser.Parse(null)).IgnoreArguments().Repeat.Once().Return(mockR);
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                var r = queryExecuter.Execute(new SolrQuery(queryString), new QueryOptions {
                    Fields = new[] {"id", "name"},
                });
            });
        }

        [Test]
        public void Facets() {
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.DynamicMock<ISolrQueryResultParser<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(() => {
                var q = new Dictionary<string, string>();
                q["q"] = "";
                q["rows"] = SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString();
                q["facet"] = "true";
                q["facet.field"] = "Id";
                q["facet.query"] = "id:[1 TO 5]";
                Expect.Call(conn.Get("/select", q))
                    .Repeat.Once().Return("");
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                queryExecuter.Execute(new SolrQuery(""), new QueryOptions {
                    FacetQueries = new ISolrFacetQuery[] {
                        new SolrFacetFieldQuery("Id"),
                        new SolrFacetQuery(new SolrQuery("id:[1 TO 5]")),
                    },
                });
            });
        }

        public KeyValuePair<T1, T2> KVP<T1, T2>(T1 a, T2 b) {
            return new KeyValuePair<T1, T2>(a, b);
        }

        [Test]
        public void MultipleFacetFields() {
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.DynamicMock<ISolrQueryResultParser<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            With.Mocks(mocks).Expecting(() => {
                var q = new List<KeyValuePair<string, string>> {
                    KVP("q", ""),
                    KVP("rows", SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString()),
                    KVP("facet", "true"),
                    KVP("facet.field", "Id"),
                    KVP("facet.field", "OtherField"),
                };
                Expect.Call(conn.Get("/select", q))
                    .Repeat.Once().Return("");
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                queryExecuter.Execute(new SolrQuery(""), new QueryOptions {
                    FacetQueries = new ISolrFacetQuery[] {
                        new SolrFacetFieldQuery("Id"),
                        new SolrFacetFieldQuery("OtherField"),
                    },
                });
            });
        }

        [Test]
        public void Highlighting() {
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var conn = mocks.CreateMock<ISolrConnection>();
            var parser = mocks.DynamicMock<ISolrQueryResultParser<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            const string highlightedField = "field1";
            const string afterTerm = "after";
            const string beforeTerm = "before";
            const int snippets = 3;
            const string alt = "alt";
            const int fragsize = 7;
            With.Mocks(mocks).Expecting(() => {
                var q = new Dictionary<string, string>();
                q["q"] = "";
                q["rows"] = SolrQueryExecuter<TestDocument>.ConstDefaultRows.ToString();
                q["hl"] = "true";
                q["hl.fl"] = highlightedField;
                q["hl.snippets"] = snippets.ToString();
                q["hl.fragsize"] = fragsize.ToString();
                q["hl.requireFieldMatch"] = "true";
                q["hl.alternateField"] = alt;
                q["hl.simple.pre"] = beforeTerm;
                q["hl.simple.post"] = afterTerm;
                q["hl.regex.slop"] = "4.12";
                q["hl.regex.pattern"] = "\\.";
                q["hl.regex.maxAnalyzedChars"] = "8000";
                Expect.Call(conn.Get("/select", q))
                    .Repeat.Once().Return("");
                Expect.Call(container.GetInstance<IListRandomizer>())
                    .Repeat.Any()
                    .Return(null);
            }).Verify(() => {
                var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper);
                queryExecuter.Execute(new SolrQuery(""), new QueryOptions {
                    Highlight = new HighlightingParameters {
                        Fields = new[] {highlightedField},
                        AfterTerm = afterTerm,
                        BeforeTerm = beforeTerm,
                        Snippets = snippets,
                        AlternateField = alt,
                        Fragsize = fragsize,
                        RequireFieldMatch = true,
                        RegexSlop = 4.12,
                        RegexPattern = "\\.",
                        RegexMaxAnalyzedChars = 8000,
                    }
                });
            });
        }

        [Test]
        public void FilterQuery() {
            var mocks = new MockRepository();
            var container = mocks.CreateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => container);
            var parser = mocks.DynamicMock<ISolrQueryResultParser<TestDocument>>();
            var mapper = mocks.CreateMock<IReadOnlyMappingManager>();
            var conn = new MockConnection(new[] {
                new KeyValuePair<string, string>("q", "*:*"),
                new KeyValuePair<string, string>("rows", "10"),
                new KeyValuePair<string, string>("fq", "id:0"),
                new KeyValuePair<string, string>("fq", "id:2"),
            });
            var queryExecuter = new SolrQueryExecuter<TestDocument>(conn, parser, mapper) {
                DefaultRows = 10,
            };
            queryExecuter.Execute(SolrQuery.All, new QueryOptions {
                FilterQueries = new[] {
                    new SolrQuery("id:0"),
                    new SolrQuery("id:2"),
                },
            });
        }
    }
}