using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using RESTfulWebInterface.Models;

using CsvHelper;
using CsvHelper.Configuration;
using System.Threading;

namespace RESTfulWebInterface.Persistence
{
    public class CsvLoader
    {
        readonly ILogger<CsvLoader> logger;

        public CsvLoader(ILogger<CsvLoader> logger)
        {
            this.logger = logger;
        }

        public IReadOnlyCollection<Person> Load(Stream stream, bool strict = true)
        {
            logger.LogInformation($"Deserializing CSV records (strict mode: {(strict ? "on" : "off")})");
            var persons = new List<Person>();
            int numberOfErrors = 0;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            if (!strict)
                stream = GetWorkaroundStream(stream);

            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<PersonMap>();
                while (csv.Read())
                {
                    try
                    {
                        var person = csv.GetRecord<Person>();
                        persons.Add(person);
                    }
                    catch (Exception ex)
                    {
                        var errorLine = csv.Parser.RawRow;
                        var errorContent = csv.Parser.RawRecord;
                        logger.LogError("CSV source has an error on line {Line}, content {Content}: {Message}", errorLine, errorContent, ex.Message);
                        if (strict)
                        {
                            throw;
                        }
                        // in non-strict mode we just ignore the failing line
                        logger.LogWarning("Ignoring failed input line {Line}, please check the data integrity", errorLine);
                        numberOfErrors++;
                    }
                }
            }

            for (int i = 0; i < persons.Count; i++)
                persons[i].Id = i + 1;

            if (numberOfErrors > 0)
                logger.LogWarning($"Deserializing CSV records finished with {numberOfErrors} error(s)");
            else
                logger.LogInformation("Deserializing CSV records finished successfully");

            return persons.AsReadOnly();
        }

        sealed class PersonMap : ClassMap<Person>
        {
            public PersonMap()
            {
                Map(m => m.LastName).Convert(args => GetTrimmedField(args, 0));
                Map(m => m.Name).Convert(args => GetTrimmedField(args, 1));
                Map(m => m.ZipCode).Convert(args => SplitZipCity(args, 0));
                Map(m => m.City).Convert(args => SplitZipCity(args, 1));
                Map(m => m.Color).Index(3);
            }

            string GetTrimmedField(ConvertFromStringArgs args, int index) =>
                args.Row.GetField(index).Trim();

            string SplitZipCity(ConvertFromStringArgs args, int partIndex)
            {
                var valueInCsv = args.Row.GetField(2);
                var splitValues = valueInCsv.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (splitValues.Length != 2)
                {
                    var rowNumber = args.Row.Parser.Row;
                    var rowContent = args.Row.Parser.RawRecord;
                    throw new FormatException($"Error in input row {rowNumber}: field must contain ZIP and City separated by a space. Row content: {rowContent}");
                }
                return splitValues[partIndex];
            }
        }

        Stream GetWorkaroundStream(Stream original)
        {
            var ms = new MemoryStream();
            using (var tw = new StreamWriter(ms, leaveOpen: true))
            using (var tr = new StreamReader(original))
            {
                string? partialLine = null;
                while (true)
                {
                    var currentLine = tr.ReadLine();
                    if (currentLine == null)
                        break;
                    if (partialLine != null)
                    {
                        currentLine = partialLine + currentLine;
                        partialLine = null;
                    }
                    if (currentLine.TrimEnd().EndsWith(','))
                        partialLine = currentLine;
                    else
                        tw.WriteLine(currentLine);
                }
                if (partialLine != null)
                    tw.WriteLine(partialLine);
            }
            ms.Position = 0;
            return ms;
        }
    }
}
