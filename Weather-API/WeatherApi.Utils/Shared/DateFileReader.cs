using System.Globalization;
using System.Text.RegularExpressions;

namespace WeatherApi.Utils.Shared
{
    public static class DateFileReader
    {
        /// <summary>
        /// Reads a file of dates, parses supported formats to ISO (yyyy-MM-dd).
        /// Invalid or unrecognized dates are recorded in the returned errors list.
        /// The method never throws on parse errors; it records them and continues.
        /// </summary>
        /// <param name="filePath">Full path to the dates file.</param>
        /// <returns>Tuple with list of ISO date strings and list of error messages.</returns>
        public static (List<string> ParsedIsoDates, List<string> Errors) ReadAndParseDates(string filePath)
        {
            var parsed = new List<string>();
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errors.Add("Provided file path is null or empty.");
                return (parsed, errors);
            }

            if (!File.Exists(filePath))
            {
                errors.Add($"File not found: {filePath}");
                return (parsed, errors);
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to read file: {ex.Message}");
                return (parsed, errors);
            }

            // Common formats expected in the file
            var formats = new[]
            {
                "MM/dd/yyyy",
                "M/d/yyyy",
                "MMMM d, yyyy",
                "MMMM dd, yyyy",
                "MMM-d-yyyy",
                "MMM-dd-yyyy",
                "MMM d, yyyy",
                "MMM dd, yyyy"
            };

            var culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < lines.Length; i++)
            {
                var raw = lines[i] ?? string.Empty;
                // Remove leading bullet or similar characters (e.g. '•', '-', '*') and trim whitespace
                var cleaned = Regex.Replace(raw, @"^\s*[\u2022\-\*\•]+\s*", string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                if (DateTime.TryParseExact(cleaned, formats, culture, DateTimeStyles.None, out var dt) ||
                    DateTime.TryParse(cleaned, culture, DateTimeStyles.None, out dt))
                {
                    // Valid date -> convert to ISO yyyy-MM-dd
                    parsed.Add(dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                else
                {
                    // Treat as invalid and record a clear error
                    errors.Add($"Line {i + 1}: '{raw}' is not a recognized or valid date.");
                }
            }

            return (parsed, errors);
        }
    }
}