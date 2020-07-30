using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections;
using System.IO;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Extensions;

namespace Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    public static class Extension
    {
        //通用排序
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty, bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            if (property.PropertyType.ToString().Contains("List"))
                property = type.GetProperty("nameSap");
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                   source.Expression, Expression.Quote(orderByExpression));
            return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
        }

        //通用搜索
        public static IQueryable<TEntity> FilterBy<TEntity>(this IQueryable<TEntity> source, string searchField, string searchOper, string searchString)
        {
            // Dictionary to map the two letter filter code supplied by jqGrid to the Linq filter mechanism for string columns
            IDictionary<string, string> stringFiltersdictionary = new Dictionary<string, string>()
            {
                //"bn" => "NOT LIKE '$params->{'searchString'}%'",
                //"en" => "NOT LIKE '%". "$params->{'searchString'}'",
                //"nc" => "!~ '$params->{'searchString'}'",
                { "bw", "StartsWith" },
                { "cn", "Contains" },
                { "eq", "Equals" },
                { "ew", "EndsWith" },
                { "nc", "Contains"},
                { "bn", "StartsWith"},
                { "en", "EndsWith"}
            };
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            var searchFieldProperty = Expression.Property(parameter, searchField);
            ConstantExpression searchStringConstant;
            Expression<Func<TEntity, bool>> lambda;
            var type = searchFieldProperty.Member.ToString();
            if (searchField == "recordTime")
            {
                var startDate = DateTime.ParseExact(searchString + " 05:00", "MM/dd/yyyy HH:mm", null);
                var endDate = startDate.AddDays(1);
                var dateConstant = Expression.Constant(startDate);
                var dateConvertedConstant = Expression.Convert(dateConstant, searchFieldProperty.Type);
                var dateFilterExpression = Expression.GreaterThanOrEqual(searchFieldProperty, dateConvertedConstant);
                lambda = Expression.Lambda<Func<TEntity, bool>>(dateFilterExpression, parameter);
                source = source.Where(lambda);
                dateConstant = Expression.Constant(endDate);
                dateConvertedConstant = Expression.Convert(dateConstant, searchFieldProperty.Type);
                dateFilterExpression = Expression.LessThan(searchFieldProperty, dateConvertedConstant);
                lambda = Expression.Lambda<Func<TEntity, bool>>(dateFilterExpression, parameter);
                return source.Where(lambda);
            }
            if (type.Contains("String"))
            {
                searchStringConstant = Expression.Constant(searchString.ToLower());
                MethodCallExpression searchFieldToLower = Expression.Call(searchFieldProperty, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                string filterType = stringFiltersdictionary["eq"]; // Default to Equal
                if (stringFiltersdictionary.ContainsKey(searchOper))
                {
                    filterType = stringFiltersdictionary[searchOper];
                }
                if ("Equals" == filterType)
                {
                    lambda = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(searchFieldToLower, searchStringConstant), parameter);
                }
                else if (searchOper == "nc" || searchOper == "bn" || searchOper == "en")
                {
                    UnaryExpression filterExpression = Expression.Not(Expression.Call(searchFieldToLower, filterType, null, Expression.Convert(searchStringConstant, searchFieldProperty.Type)));
                    lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                }
                else
                {
                    MethodCallExpression filterExpression = Expression.Call(searchFieldToLower, filterType, null, Expression.Convert(searchStringConstant, searchFieldProperty.Type));
                    lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
                }
            }
            else // The column on which we are sorting must be numeric
            {
                if (type.Contains("Decimal"))
                {
                    searchStringConstant = Expression.Constant(decimal.Parse(searchString));
                }
                else if (type.Contains("Int"))
                {
                    searchStringConstant = Expression.Constant(Convert.ToInt32(searchString));
                }
                else if (type.Contains("DateTimeOffset"))
                {
                    searchStringConstant = Expression.Constant(DateTimeOffset.Parse(searchString));
                }
                else
                {
                    searchStringConstant = Expression.Constant((searchString == "true") ? true : false);
                }
                var convertedConstant = Expression.Convert(searchStringConstant, searchFieldProperty.Type);
                BinaryExpression filterExpression = Expression.Equal(searchFieldProperty, convertedConstant);
                switch (searchOper)
                {
                    case "ge": // Greater than or Equal
                        filterExpression = Expression.GreaterThanOrEqual(searchFieldProperty, convertedConstant);
                        break;
                    case "gt": // Greater than
                        filterExpression = Expression.GreaterThan(searchFieldProperty, convertedConstant);
                        break;
                    case "le": // Less than or Equal
                        filterExpression = Expression.LessThanOrEqual(searchFieldProperty, convertedConstant);
                        break;
                    case "lt": // Less than
                        filterExpression = Expression.LessThan(searchFieldProperty, convertedConstant);
                        break;
                    case "ne": // Not Equal
                        filterExpression = Expression.NotEqual(searchFieldProperty, convertedConstant);
                        break;
                }
                lambda = Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
            }
            return source.Where(lambda);
        }

    }
}

namespace PcrSarenBot.Controllers
{
    public class GridHelperController<VIEW_MODEL_TYPE, MODEL_TYPE> : Controller
        where VIEW_MODEL_TYPE : new()
    {
        public VIEW_MODEL_TYPE viewModel;

        public IQueryable<MODEL_TYPE> GetProcessedQuery(IQueryCollection query)
        {
            var sord = query["sord"].ToString(); // The sort order passed in by jqGrid
            var sidx = query["sidx"].ToString(); // The column on which to perform the sort
            var viewModel = Activator.CreateInstance(typeof(VIEW_MODEL_TYPE), new object[] { query });
            var rows = Convert.ToInt32(query["rows"]); // Count of rows to be retrieved passed in by jqGrid
            var page = Convert.ToInt32(query["page"]); // The page of data to be retireved passed in by jqGrid. Page is synonymous with data set count when the complete data set is broken into partial sets each with "rows" count of rows. For example, if rows is 10 and there are 78 rows total there are 8 deparate sets. Set 2 will contain rows 11 to 20.

            IQueryable<MODEL_TYPE> baseQuery = (IQueryable<MODEL_TYPE>)typeof(VIEW_MODEL_TYPE).GetProperty("baseQuery").GetValue(viewModel);
            if (query["_search"].ToString() == "true")
            {
                if (query.ContainsKey("filters") && "" != query["filters"].ToString())
                {
                    var filter = Newtonsoft.Json.JsonConvert.DeserializeObject<PcrSarenBot.Models.JqGridFilterModel>(query["filters"].ToString());
                    foreach (var rule in filter.rules)
                    {
                        // If a date range was passed in (i.e. "dr") then split it into two separate date filters (i.e. >= and <=).
                        if ("dr" == rule.op)
                        {
                            string[] dates = rule.data.Split('-');
                            if (null != dates[0])
                                baseQuery = baseQuery.FilterBy(rule.field, "ge", dates[0].Trim());
                            if (null != dates[1])
                                baseQuery = baseQuery.FilterBy(rule.field, "le", dates[1].Trim());
                        }
                        else
                        {
                            baseQuery = baseQuery.FilterBy(rule.field, rule.op, rule.data);
                        }
                    }
                }
                else if (query.ContainsKey("searchField") &&
                         query.ContainsKey("searchOper") &&
                         query.ContainsKey("searchString"))
                {
                    var searchField = query["searchField"].ToString();
                    var searchOper = query["searchOper"].ToString();
                    var searchString = query["searchString"].ToString();
                    baseQuery = baseQuery.FilterBy(searchField, searchOper, searchString);
                }
                else
                {
                    // TODO: The "_search" was supplied, but no search criteria was found??? Handle the error here!
                }
            }
            if (query.ContainsKey("secondarySearchField") &&
                query.ContainsKey("secondarySearchOper") &&
                query.ContainsKey("secondarySearchString"))
            {
                baseQuery = baseQuery.FilterBy(query["secondarySearchField"].ToString(), query["secondarySearchOper"].ToString(), query["secondarySearchString"].ToString());
            }
            if ((null != sidx) && (!Regex.IsMatch(sidx, @"^\s*$")))
            {
                Boolean descending = true;
                if (sord == "asc")
                    descending = false;
                baseQuery = baseQuery.OrderBy(sidx, descending);
            }
            return baseQuery;
        }

        public JsonResult GetJsonData()
        {
            var proccessedQuery = GetProcessedQuery(Request.Query);

            var sidx = Request.Query["sidx"].ToString(); // The column on which to perform the sort
            var rows = Convert.ToInt32(Request.Query["rows"]); // Count of rows to be retrieved passed in by jqGrid
            var page = Convert.ToInt32(Request.Query["page"]); // The page of data to be retireved passed in by jqGrid. Page is synonymous with data set count when the complete data set is broken into partial sets each with "rows" count of rows. For example, if rows is 10 and there are 78 rows total there are 8 deparate sets. Set 2 will contain rows 11 to 20.
            int totalRecords = proccessedQuery.Count();

            // Skip is only supported if the query is sorted.
            // TODO: This limitation is not in the database; eliminate this limitation.
            if (Request.Query.ContainsKey("sidx") && (!Regex.IsMatch(Request.Query["sidx"].ToString(), @"^\s*$")))
            {
                proccessedQuery = proccessedQuery.Skip((page - 1) * rows).Take(rows);
            }
            var result = proccessedQuery.ToList();
            //if (typeof(VIEW_MODEL_TYPE).Name == "ShipmentModel")
            //{

            //}
            var jsonData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)rows),
                page,
                records = totalRecords,
                rows = result
            };
            return Json(jsonData);
        }
    }
}