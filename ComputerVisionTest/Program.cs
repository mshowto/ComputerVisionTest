﻿using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComputerVisionTest
{
    class Program
    {
        // Add your Computer Vision subscription key and endpoint to your environment variables. 
        // Close/reopen your project for them to take effect.
        //static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        //static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        static string subscriptionKey = "";
        static string endpoint = "";



        // URL image used for analyzing an image (image of puppy)
        private const string ANALYZE_URL_IMAGE = "https://ncpublicdata.blob.core.windows.net/images/onury.jpg";

        // URL image for OCR (optical character recognition). (Image of motivational meme).
        private const string EXTRACT_TEXT_URL_IMAGE = "https://ncpublicdata.blob.core.windows.net/ocr/JM-poems_Bulgaria_Ognian-Asenov.png";


        static void Main(string[] args)
        {


            // Create a client
            ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

            // Analyze an image to get features and other properties.
            AnalyzeImageUrl(client, ANALYZE_URL_IMAGE).Wait();
            //BatchReadFileUrl(client, EXTRACT_TEXT_URL_IMAGE).Wait();


        }

        /*
            * AUTHENTICATE
            * Creates a Computer Vision client used by each example.
        */
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        /* 
            * ANALYZE IMAGE - URL IMAGE
            * Analyze URL image. Extracts captions, categories, tags, objects, faces, racy/adult content,
            * brands, celebrities, landmarks, color scheme, and image types.
         */
        public static async Task AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("ANALYZE IMAGE - URL");
            Console.WriteLine();

            // Creating a list that defines the features to be extracted from the image. 
            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
                 VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                 VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                 VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                 VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                 VisualFeatureTypes.Objects
            };

            Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();
            // Analyze the URL image 
            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, features);
            // Sunmarizes the image content.
            Console.WriteLine("Summary:");
            foreach (var caption in results.Description.Captions)
            {
                Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
            }
            Console.WriteLine();

            // Display categories the image is divided into.
            Console.WriteLine("Categories:");
            foreach (var category in results.Categories)
            {
                Console.WriteLine($"{category.Name} with confidence {category.Score}");
            }
            Console.WriteLine();

            // Image tags and their confidence score
            Console.WriteLine("Tags:");
            foreach (var tag in results.Tags)
            {
                Console.WriteLine($"{tag.Name} {tag.Confidence}");
            }
            Console.WriteLine();

            // Objects
            Console.WriteLine("Objects:");
            foreach (var obj in results.Objects)
            {
                Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                  $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
            }
            Console.WriteLine();

            // Well-known (or custom, if set) brands.
            Console.WriteLine("Brands:");
            foreach (var brand in results.Brands)
            {
                Console.WriteLine($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                  $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");
            }
            Console.WriteLine();

            // Faces
            Console.WriteLine("Faces:");
            foreach (var face in results.Faces)
            {
                Console.WriteLine($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, " +
                  $"{face.FaceRectangle.Left}, {face.FaceRectangle.Top + face.FaceRectangle.Width}, " +
                  $"{face.FaceRectangle.Top + face.FaceRectangle.Height}");
            }
            Console.WriteLine();

            // Identifies the color scheme.
            Console.WriteLine("Color Scheme:");
            Console.WriteLine("Is black and white?: " + results.Color.IsBWImg);
            Console.WriteLine("Accent color: " + results.Color.AccentColor);
            Console.WriteLine("Dominant background color: " + results.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color: " + results.Color.DominantColorForeground);
            Console.WriteLine("Dominant colors: " + string.Join(",", results.Color.DominantColors));
            Console.WriteLine();

            // Celebrities in image, if any.
            Console.WriteLine("Celebrities:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Celebrities != null)
                {
                    foreach (var celeb in category.Detail.Celebrities)
                    {
                        Console.WriteLine($"{celeb.Name} with confidence {celeb.Confidence} at location {celeb.FaceRectangle.Left}, " +
                          $"{celeb.FaceRectangle.Top}, {celeb.FaceRectangle.Height}, {celeb.FaceRectangle.Width}");
                    }
                }
            }
            Console.WriteLine();

            // Popular landmarks in image, if any.
            Console.WriteLine("Landmarks:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Landmarks != null)
                {
                    foreach (var landmark in category.Detail.Landmarks)
                    {
                        Console.WriteLine($"{landmark.Name} with confidence {landmark.Confidence}");
                    }
                }
            }
            Console.WriteLine();

            // Detects the image types.
            Console.WriteLine("Image Type:");
            Console.WriteLine("Clip Art Type: " + results.ImageType.ClipArtType);
            Console.WriteLine("Line Drawing Type: " + results.ImageType.LineDrawingType);
            Console.WriteLine();



        }

        /*
            * BATCH READ FILE - URL IMAGE
            * Recognizes handwritten text. 
            * This API call offers an improvement of results over the Recognize Text calls.
        */
        public static async Task BatchReadFileUrl(ComputerVisionClient client, string urlImage)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("BATCH READ FILE - URL IMAGE");
            Console.WriteLine();

            // Read text from URL
            BatchReadFileHeaders textHeaders = await client.BatchReadFileAsync(urlImage);
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;

            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            // Delay is between iterations and tries a maximum of 10 times.
            int i = 0;
            int maxRetries = 10;
            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL image {Path.GetFileName(urlImage)}...");
            Console.WriteLine();
            do
            {
                results = await client.GetReadOperationResultAsync(operationId);
                Console.WriteLine("Server status: {0}, waiting {1} seconds...", results.Status, i);
                await Task.Delay(1000);
                if (i == 9)
                {
                    Console.WriteLine("Server timed out.");
                }
            }
            while ((results.Status == TextOperationStatusCodes.Running ||
                results.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries);

            // Display the found text.
            Console.WriteLine();
            var textRecognitionLocalFileResults = results.RecognitionResults;
            foreach (TextRecognitionResult recResult in textRecognitionLocalFileResults)
            {
                foreach (Line line in recResult.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }

    }
 }

