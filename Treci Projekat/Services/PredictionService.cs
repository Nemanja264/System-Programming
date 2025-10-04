using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;


namespace Treci_Projekat.Services
{
    public class PredictionService
    {
        private readonly MLContext _ml;
        private readonly PredictionEngine<InputRow, OutputRow> _engine;

        public PredictionService(MLContext ml, ITransformer model)
        {
            _ml = ml;
            _engine = _ml.Model.CreatePredictionEngine<InputRow, OutputRow>(model);
        }

        public static PredictionService Create(string modelPath, string trainingDataPath)
        {
            var ml = new MLContext(seed: 42);

            if (File.Exists(modelPath))
            {
                using var fs = File.OpenRead(modelPath);
                var mod = ml.Model.Load(fs, out _);
                return new PredictionService(ml, mod);
            }

            if (!File.Exists(trainingDataPath))
                throw new FileNotFoundException("Training data is not found");

            var raw = ml.Data.LoadFromTextFile<InputRow>(trainingDataPath, hasHeader: true, separatorChar: '\t');
            var data = ml.Data.Cache(raw);
            var split = ml.Data.TrainTestSplit(data, testFraction:0.2 , seed: 42);

            var pipeline = ml.Transforms.Text.FeaturizeText("Features", nameof(InputRow.Text))
                .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: nameof(InputRow.Label),
                    featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);

            Directory.CreateDirectory(Path.GetDirectoryName(modelPath) ?? ".");

            using var outFS = File.Create(modelPath);
            ml.Model.Save(model, split.TrainSet.Schema, outFS);

            return new PredictionService(ml, model);
        }

        public (bool Predicted, float Probability) Predict(string text)
        {
            var o = _engine.Predict(new InputRow { Text = text });
            return (o.PredictedLabel, o.Probability);
        }
        public sealed class InputRow
        {
            [LoadColumn(0)] public string Text { get; set; } = "";
            [LoadColumn(1)] public bool Label { get; set; }
        }

        public sealed class OutputRow
        {
            [ColumnName("PredictedLabel")] public bool PredictedLabel { get; set; }
            public float Probability { get; set; }
            public float Score { get; set; }
        }

    }
}
