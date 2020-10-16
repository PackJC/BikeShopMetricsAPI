//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:		Metrics
//	File Name:		Tools.cs
//	Description:	Static methods to manipulate data for Metrics queries
//	Course:			CSCI 4350-941 - Software Engineering II
//	Authors:		Tyler Simpson: simpsontd@etsu.edu,Habtamu Tegegne: tegegne@etsu.edu
//	Created:		Monday, October 12, 2020
//	Copyright:		Tyler Simpson, Habtamu Tegegne 2020
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;

namespace BikeShopAPI.Repositories
{
    /// <summary>
    /// Utility Methods
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Convert a JsonElement to a KeyValuePair dictionary while maintaining quotes required for database query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(JsonElement input)
        {
            string jsonText = input.ToString();
            string key = "";
            string value = "";
            Dictionary<string, string> output = new Dictionary<string, string>();
            //iterate through string until '\" is found. This is the start of the first key
            for (int i = 0; i < jsonText.Length; i++)
            {
                char c = jsonText[i];
                if (jsonText [i] == '\"')
                {
                    //key found
                    int j = 1; //offset to first char of key
                    while(true)
                    {
                        if(jsonText[i+j] == '\"')
                        {
                            //end of key found. start value
                            if(jsonText[i + j + 3] != '\"')
                            {
                                j += 3;
                                //value is a number. Read until ',' found.
                                while(jsonText[i+j] != ',' && jsonText[i+j] != '\r')
                                {
                                    value += jsonText[i + j];
                                    j++;
                                }
                                //key:value pair has been found. Search for next.
                                output.Add(key, value);
                                key = "";
                                value = "";
                                i += j;
                                break;
                            }
                            else
                            {
                                //value is a string
                                j += 4;
                                value += '\'';
                                while(jsonText[i + j] != '\"')
                                {
                                    value += jsonText[i + j];
                                    j++;
                                }
                                value += '\'';
                                //key:value pair has been found. Search for next
                                output.Add(key, value);
                                key = "";
                                value = "";
                                i += j;
                                break;

                            }
                        }
                        else
                        {
                            key += jsonText[i + j];
                            j++;
                        }
                    }
                }
            }
            output = ConvertDates(output);
            return output;
        }
        /// <summary>
        /// Convert date format "YYYY-MM-DDT00:00:00" to "dd-MMM-yy because Oracle database does not accept the format it outputs as input. What a Hypocrite.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertDates(Dictionary<string, string> input)
        {
            Regex date = new Regex(@"'(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})'");
            Dictionary<string, string> output = new Dictionary<string, string>(input);
            foreach(KeyValuePair<string, string> entry in input)
            {
                if (date.IsMatch(entry.Value))
                {
                    DateTime dateString = DateTime.Parse(entry.Value.Substring(1,10));
                    output[entry.Key] = '\'' + dateString.ToString("dd-MMMM-yy") + '\'';
                }
            }
            return output;
        }
    }
}
