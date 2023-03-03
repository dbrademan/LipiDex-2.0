using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using System.Data.SQLite;



namespace LipiDex2.PeakFinder 
{
    public class PeakFinder  
    {
        //Constants
        //public double MINPPMDIFF = 20.0;
        //public double MINRTMULTIPLIER = 1.0;
        //public double MINDOTPRODUCT = 500.0;
        //public double MINREVDOTPRODUCT = 700.0;
        //public double MINFAPURITY = 75.0;
        //public int MINIDNUM = 1;

        //static List<Sample> samples = new List<Sample>();
        static Dictionary<int, Sample> sampleMap = new Dictionary< int, Sample>();
        //static List<CompoundGroup> compoundGroups = new List<CompoundGroup>();
        static Dictionary<int, CompoundGroup> compoundGroupMap = new Dictionary<int, CompoundGroup>();

        //static int[] cGIndexArray = new int[3]; // TODO: Replace with dictionary or SQLite column mapper object
        //static int[] cIndexArray = new int[8];     // TODO: Replace                                            
        //static int[] fGIndexArray = new int[10];  // TODO: Replace 

        //string cdResultsFilepath;
        // string featureFileString Don't need
        // List<File> resultFiles   not needed

        public string compoundsTableName = "ConsolidatedUnknownCompoundItems";
        public string compoundsPerFileTableName = "UnknownCompoundInstanceItems";
        public string featuresTableName = null;
        private object exception;

        //Constructor
        public PeakFinder()
        { 
            ReadSqlite(false);
        }


        public void ReadSqlite(bool shortTest)
        {
            string filepath = @"D:\Lipidex2\data\CD\Nilerat_lipids_CD33_aligned.cdResult";
            string connectionString = $"URI=file:{filepath}";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string limitRows = "";
                int numLimitRows = 3;
                if (shortTest) { limitRows = $"LIMIT {numLimitRows}"; }

                // ########## READ STUDY INFORMATION TABLE
                string studyInformationCommand = $@"SELECT * FROM StudyInformation";
                using (SQLiteCommand command = new SQLiteCommand(studyInformationCommand, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Because you have the option to upload multiple files into CD, but then pick and choose certain files for a workflow,
                            //     the ID (integer, 43) and the Sample (S + integer, "S43") and the StudyFileID (F + integer, "F43") 
                            //     might have different integer values, and this could confuse things

                            // This table includes any file metadata variables you created in the Study Creator dialogue in CD.
                            //     These columns begin with "CF" then have your variable name, e.g. "CFanimal", "CFsampling", "CFsex"
                            //     It might be convenient to parse these CFcolumns for use in LipiDex,
                            //     or we could just allow people to specify groupings in LipiDex interface.

                            sampleMap.Add(
                                (int)reader["ID"], 
                                new Sample(
                                    (string)reader["SampleIdentifier"],
                                    (string)reader["FileName"],
                                    (int)reader["ID"],
                                    (string)reader["Sample"],
                                    (string)reader["StudyFileID"],
                                    0,
                                    false
                                    )
                                );
                        }
                    }
                }
                // ########## READ COMPOUNDS TABLE
                string compoundsTableCommand = $@"SELECT * FROM {compoundsTableName} {limitRows}";  
                using (SQLiteCommand command = new SQLiteCommand(compoundsTableCommand, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader()) 
                    {
                        while (reader.Read())
                        {
                            // For each row of reader, create one CompoundGroup
                            // Read all the per-file quantitation blobs

                            byte[] areasArr =         (byte[])reader["Area"];
                            byte[] pqfFwhmArr =       (byte[])reader["PQFFWHM2Base"];
                            byte[] pqfJaggArr =       (byte[])reader["PQFJaggedness"];
                            byte[] pqfModalityArr =   (byte[])reader["PQFModality"];
                            byte[] pqfZigZagArr =     (byte[])reader["PQFZigZagIndex"];
                            byte[] pqfPeakRatingArr = (byte[])reader["PeakRating"];
                            byte[] gapStatusArr =     (byte[])reader["GapStatus"];
                            //byte[] gapFillStatusArr = (byte[])reader["GapFillStatus"];  //Sometimes GapFillStatus is a null, so skip it

                            // Make a FileResult object for each entry and populate fileResults
                            List<FileResult> fileResults = new List<FileResult>();
                            // gapStatus Arrays are byte arrays of Singles + separator byte, so use another iterator j+=5
                            for (int i = 0, j = 0; i < areasArr.Length; i+=9, j+=5)
                            {
                                FileResult cmpdFileResult = new FileResult(
                                    new Sample("my_filename", "my_filepath", 10, "my_sample_id", "my_study_file_id", 0, false),
                                    BitConverter.ToDouble(areasArr, i),
                                    BitConverter.ToDouble(pqfFwhmArr, i),
                                    BitConverter.ToDouble(pqfJaggArr, i),
                                    BitConverter.ToDouble(pqfModalityArr, i),
                                    BitConverter.ToDouble(pqfZigZagArr, i),
                                    BitConverter.ToDouble(pqfPeakRatingArr, i),
                                    BitConverter.ToSingle(gapStatusArr, j)
                                    //BitConverter.ToSingle(gapFillStatusArr, j)
                                    );

                                fileResults.Add(cmpdFileResult);
                            }
                            // TODO: enforcing type like (int) or (double) is brittle, SQLite should check types and raise error
                            CompoundGroup cmpdGroup = new CompoundGroup(
                                (int)reader["ID"],
                                (double)reader["MolecularWeight"],
                                (double)reader["MassOverCharge"],
                                (double)reader["RetentionTime"],
                                (double)reader["MaxArea"],
                                fileResults,
                                (int)reader["MSnStatus"],
                                (int)reader["MSDepth"],
                                (int)reader["Checked"],
                                (string)reader["ReferenceIon"],
                                (string)reader["Name"]
                                );
                            compoundGroupMap.Add((int)reader["ID"], cmpdGroup);

                            string y = "asdf";
                        }
                    }
                }
                                
                // Important, sort the compoundGroups List to enable accession by index
                // ACCESSION BY INDEX IS NOT GREAT BECAUSE THE COMPOUND DISCOVERER INDEX IS 1-INDEXED
                //compoundGroups.Sort((x, y) => x.compoundGroupID.CompareTo(y.compoundGroupID));

                string t = "";
                

                // ######### READ COMPOUND SIMILARITY ITEMS TABLE
                // ######### READ COMPOUND SIMILARITY --> COMPOUND GROUPS LINKER TABLE
                // In my Nile rat lipids 3.3 run, with 67 files, this table has 900k rows
                // Reading this table can avoid having LipiDex do the Feature Filtering steps that is O(n^3) with triple nested for loops
                //     because CD already did this for us.
                // 
                // The number of rows in linker table is exactly 2x CompoundSimilarityItems table

                string compoundSimilarityTableCommand = $@"SELECT * FROM compoundSimilarityItems";
                string similarityLinkerTableCommand = $@"SELECT * FROM CompoundSimilarityItemsConsolidatedUnknownCompoundItems";
                using (SQLiteCommand similarityCommand = new SQLiteCommand(compoundSimilarityTableCommand, connection))
                {
                    using (SQLiteCommand linkerCommand = new SQLiteCommand(similarityLinkerTableCommand, connection))
                    {
                        using (SQLiteDataReader simReader = similarityCommand.ExecuteReader())
                        {
                            using (SQLiteDataReader linkerReader = linkerCommand.ExecuteReader())
                            {
                                // For each compound similarity item:
                                // 1. Make a CompoundSimilarity() object with similarityID,
                                //                                            MassShift,
                                //                                            TransformationString,
                                //                                            CompositionChange  (e.g. H2O),
                                //                                            TransformationMass (mass shift of the composition change),
                                //                                            MSnScore
                                //                                            ForwardCoverage (double) 
                                //                                            ForwardMatches (int)
                                //                                            ReverseCoverage
                                //                                            ReverseMatches (int)
                                //                                            Calculate ppmError for MassShift and TransformationMass
                                // 2. For both compound groups, do CompoundGroup.SimilarCompoundList.Add(CompoundSimilarity object)
                                while (simReader.Read())
                                {
                                    linkerReader.Read();
                                    int similarityID = (int)linkerReader["CompoundSimilarityItemsID"];  
                                    int compoundGroupID1 = (int)linkerReader["ConsolidatedUnknownCompoundItemsID"];
                                    linkerReader.Read();
                                    //int similarityID2 = (int)linkerReader["CompoundSimilarityItemsID"];
                                    //if (similarityID != similarityID2) { throw new Exception exception; } similarityID2 should be the same as similarityID, but might want to check
                                    int compoundGroupID2 = (int)linkerReader["ConsolidatedUnknownCompoundItemsID"];

                                    SimilarCompound sc = new SimilarCompound(
                                        (int)simReader["ID"],
                                        (double)simReader["MassShift"],
                                        //(double)simReader["TransformationMass"],
                                        //(string)simReader["TransformationString"],
                                        //(string)simReader["CompositionChange"],
                                        //(double)simReader["MSnScore"],
                                        //(double)simReader["ForwardCoverage"],
                                        //(double)simReader["ReverseCoverage"],
                                        //(int)simReader["ForwardMatches"],
                                        //(int)simReader["ReverseMatches"],
                                        compoundGroupID1, 
                                        compoundGroupID2
                                        );

                                    compoundGroupMap[compoundGroupID1].similarCompoundList.Add(sc);
                                    compoundGroupMap[compoundGroupID2].similarCompoundList.Add(sc);
                                }
                                
                            }
                        }
                    }
                }

                //using (SQLiteCommand command = new SQLiteCommand("SELECT SQLITE_VERSION()", connection))
                //{
                //    string version = command.ExecuteScalar().ToString();
                //    Console.WriteLine($"SQLIte version: {version}");

                //}
                string asdf = "asdf";
            }

        }

        public void ParseBlob(string colName, bool isDouble, int numSamples)
        {
            //
        }

    }





}
