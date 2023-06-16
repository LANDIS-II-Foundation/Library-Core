using Landis.Utilities;
using System.Collections.Generic;

using Landis.Core;
using System;

namespace Landis.Species
{
    /// <summary>
    /// A parser that reads a dataset of species parameter from text input.
    /// </summary>
    public class DatasetParser
        : Landis.TextParser<ISpeciesDataset>
    {
        public override string LandisDataValue
        {
            get {
                return "Species";
            }
        }

        //---------------------------------------------------------------------

        static DatasetParser()
        {
            PostFireRegenerationUtil.RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        public DatasetParser()
        {
        }

        //---------------------------------------------------------------------

        protected override ISpeciesDataset Parse()
        {
            ReadLandisDataVar();

            IEditableDataset dataset = new EditableDataset();
            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();

            InputVar<string> name = new InputVar<string>("Name");
            InputVar<string> fullName = new InputVar<string>("Full Name");
            InputVar<int> longevity = new InputVar<int>("Longevity");
            InputVar<int> maturity = new InputVar<int>("Sexual Maturity");
            InputVar<int> effectiveSeedDist = new InputVar<int>("Effective Seed Dist",
                                                                EffectiveSeedDist.ReadMethod);
            InputVar<int> maxSeedDist = new InputVar<int>("Max Seed Dist");
            InputVar<float> vegReprodProb = new InputVar<float>("Vegetative Reprod Prob");
            InputVar<int> minSproutAge = new InputVar<int>("Min Sprout Age");
            InputVar<int> maxSproutAge = new InputVar<int>("Max Sprout Age");
            InputVar<PostFireRegeneration> postFireRegen = new InputVar<PostFireRegeneration>("Post-Fire Regen");

            while (! AtEndOfInput) {
                IEditableParameters parameters = new EditableParameters();
                dataset.Add(parameters);

                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(name, currentLine);
                int lineNumber;
                if (lineNumbers.TryGetValue(name.Value.Actual, out lineNumber))
                    throw new InputValueException(name.Value.String,
                                                  "The name \"{0}\" was previously used on line {1}",
                                                  name.Value.Actual, lineNumber);
                else
                    lineNumbers[name.Value.Actual] = LineNumber;
                parameters.Name = name.Value;

                ReadValue(longevity, currentLine);
                parameters.Longevity = longevity.Value;

                ReadValue(maturity, currentLine);
                parameters.Maturity = maturity.Value;

                ReadValue(effectiveSeedDist, currentLine);
                parameters.EffectiveSeedDist = effectiveSeedDist.Value;

                ReadValue(maxSeedDist, currentLine);
                parameters.MaxSeedDist = maxSeedDist.Value;

                ReadValue(vegReprodProb, currentLine);
                parameters.VegReprodProb = vegReprodProb.Value;

                ReadValue(minSproutAge, currentLine);
                parameters.MinSproutAge = minSproutAge.Value;

                ReadValue(maxSproutAge, currentLine);
                parameters.MaxSproutAge = maxSproutAge.Value;

                ReadValue(postFireRegen, currentLine);
                parameters.PostFireRegeneration = postFireRegen.Value;

                if (currentLine.Index != -1)
                {
                    if (ReadOptionalWord(currentLine, fullName))
                    {
                        parameters.FullName = fullName.Value;

                        // If adding another optional parameter, move this bottom code out to the next one
                        CheckNoDataAfter("the " + fullName.Name + " column",
                                     currentLine);
                    }
                }
                else
                {
                    CheckNoDataAfter("the " + postFireRegen.Name + " column",
                                 currentLine);
                }

                GetNextLine();
            }

            return dataset.GetComplete();
        }

        private bool ReadOptionalWord(StringReader currentLine, InputVar<string> var)
        {
            int i = 0;
            while ((char)currentLine.Peek() != ushort.MaxValue && i != -1 && char.IsWhiteSpace((char)currentLine.Peek()))
            {
                i = currentLine.Read();
            }

            if ((char)currentLine.Peek() == ushort.MaxValue || i == -1)
            {
                return false;
            }
            var.ReadValue(currentLine);

            return !string.IsNullOrEmpty(var.Value.Actual);
        }
    }
}
