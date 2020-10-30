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
            //split input on field
            List<string> fields = jsonText.Split(',').ToList();
            for (int i = 0; i < fields.Count; i++)
            {
                if (!fields[i].Contains(':'))
                {
                    fields[i - 1] += "," + fields[i];
                    fields.RemoveAt(i);
                }
            }
            //clean up each field and add to dictionary
            foreach (string s in fields)
            {
                bool keyStartFound = false;
                bool keyEndFound = false;
                bool valueStartFound = false;
                bool valueEndFound = false;
                bool isNumeric = false;
                for (int i = 0; i < s.Length; i++)
                {
                    if (!keyStartFound)
                    {
                        //check if char is number or quote. True - add to key and start keyStartFound true. Else do nothing
                        if (s[i] == '"' || Char.IsNumber(s[i]))
                        {
                            keyStartFound = true;
                            if (Char.IsNumber(s[i]))
                            {
                                key += s[i];
                            }
                            if (Char.IsNumber(s[i])) isNumeric = true;
                        }
                    }
                    else if (keyStartFound && !keyEndFound)
                    {
                        if (s[i] == '"') //Text key
                        {
                            keyEndFound = true;
                        }
                        else if (isNumeric && !Char.IsNumber(s[i]) && s[i] != '.' ) //numeric Key (should not happen but good to have functionality)
                        {
                            keyEndFound = true;
                            isNumeric = false; //reset for value
                        }
                        else
                        {
                            key += s[i];
                        }
                    }
                    else if (keyStartFound && keyEndFound && !valueStartFound)
                    {
                        if (s[i] == '"' || Char.IsNumber(s[i]))
                        {
                            valueStartFound = true;
                            if (Char.IsNumber(s[i]))
                            {
                                isNumeric = true;
                                value += s[i];
                            }
                            else
                            {
                                value += '\'';
                            }
                            
                            if (i == s.Length - 1)
                            {
                                output.Add(key, value);
                                break;
                            }
                        }
                    }
                    else if (keyStartFound && keyEndFound && valueStartFound && !valueEndFound)
                    {
                        if (s[i] == '"') //Text value
                        {
                            value += '\'';
                            valueEndFound = true;
                            output.Add(key, value);
                            break;
                        }
                        else if ((isNumeric && !Char.IsNumber(s[i]) && s[i] != '.') || i == s.Length - 1) //numeric value
                        {
                            if (i == s.Length - 1) value += s[i];
                            valueEndFound = true;
                            if (value.Last() == '}')
                            {
                                value = value.Remove(value.Length - 1, 1); //remove '}' because it is breaking the code and I don't like it
                            }
                            output.Add(key, value);
                            break;
                        }
                        else
                        {
                            value += s[i];
                        }
                    }
                }
                //reset variables
                key = "";
                value = "";
                keyStartFound = false;
                keyEndFound = false;
                valueStartFound = false;
                valueEndFound = false;
                isNumeric = false;
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
            Regex timeStamp = new Regex(@"'(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})'");
            Dictionary<string, string> output = new Dictionary<string, string>(input);
            foreach(KeyValuePair<string, string> entry in input)
            {
                if (date.IsMatch(entry.Value))
                {
                    string value = "";
                    foreach (char c in entry.Value)
                    {
                        if (c == 'T')
                        {
                            value += " ";
                        }
                        else
                        {
                            value += c;
                        }
                    }
                    value = "TO_DATE(" + value + @", 'YYYY-MM-DD HH24:MI:SS')";
                    output[entry.Key] = value;

                }
                if (timeStamp.IsMatch(entry.Value))
                {
                    string value = "TO_DATE(" + entry.Value + @", 'YYYY-MM-DD HH24:MI:SS')";
                    output[entry.Key] = value;
                }
            }
            return output;
        }
    }
}
