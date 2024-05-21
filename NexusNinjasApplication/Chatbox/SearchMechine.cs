using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SearchMachine
{
    public class EncryptionHelper
    {
        private X509Certificate2 certificate;

        public EncryptionHelper(string certificatePath, string password)
        {
            certificate = new X509Certificate2(certificatePath, password);
        }

        public string EncryptJson(string jsonData)
        {
            using (RSA rsa = certificate.GetRSAPublicKey())
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                byte[] encryptedBytes = rsa.Encrypt(jsonBytes, RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public string EncryptJsonFile(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            return EncryptJson(jsonData);
        }
    }

    public class ApiService
    {
        private readonly string apiKey;
        private readonly JsonFileHelper jsonFileHelper;

        public ApiService(string apiKey, JsonFileHelper jsonFileHelper)
        {
            this.apiKey = apiKey;
            this.jsonFileHelper = jsonFileHelper;
        }

        public async Task<string> GenerateContent(string message)
        {
            try
            {
                // Normalize and check the question
                string localAnswer = jsonFileHelper.GetAnswer(message);
                if (localAnswer != "Question not found.")
                {
                    return localAnswer;
                }

                // Extract relevant data from JsonFileHelper based on keywords
                string extractedData = jsonFileHelper.GetRelevantData(message);

                // If no relevant data is found, use the AI to generate content
                var genai = new GoogleAI("AIzaSyDjtVeMDb6Oi-vF45BhynkUP1ioToFnNq0");
                var generationConfig = new GenerationConfig
                {
                    Temperature = 0.15f,
                    TopP = 1f,
                    MaxOutputTokens = 1500
                };
                var model = genai.GenerativeModel(model: Model.Gemini15Pro, generationConfig: generationConfig);
                var response = await model.GenerateContent(extractedData + " " + message);

                // Ensure the response is relevant to UTS Sarawak
                string filteredResponse = FilterResponse(response.Text);
                return filteredResponse;
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception as needed
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                return "An error occurred while fetching the response. Please try again later.";
            }
        }

        private string FilterResponse(string response)
        {
            // Check for mentions of University Technology Sarawak in various forms
            if (response.Contains("UTS Sarawak") || response.Contains("University Technology Sarawak") || response.Contains("UTS"))
            {
                return response;
            }

            // If none of the conditions are met, return the response unchanged
            return response;
        }
    }

    public class JsonFileHelper
    {
        private readonly Dictionary<string, string> jsonData = new Dictionary<string, string>
        {
            {"what programs are offered", @"
            {
                ""program"":{
                    ""foundation"": [
                    ""Foundation in Art"",
                    ""Foundation in Science""
                ],
                ""undergraduate"": {
                    ""engineering_and_technology"": [
                        ""Bachelor of Mechanical Engineering (Hons)"",
                        ""Bachelor of Civil Engineering (Hons)"",
                        ""Bachelor of Electrical Engineering (Hons)"",
                        ""Bachelor of Food Technology (Hons)"",
                        ""Bachelor of Science in Occupational Safety And Health (Hons)""
                    ],
                    ""business_and_management"": [
                        ""Bachelor of Business Administration (Hons)"",
                        ""Bachelor of Accountancy (Honours)"",
                        ""Bachelor of Business in Marketing (Honours)"",
                        ""Bachelor of Technology Management (Honours)""
                    ],
                    ""built_environment"": [
                        ""Bachelor of Science in Architecture (Hons)"",
                        ""Bachelor of Quantity Surveying (Hons)"",
                        ""Bachelor of Art Interior Design (Hons)"",
                        ""Bachelor of Science in Property and Construction Management (Hons)""
                    ],
                    ""computing_and_creative_media"": [
                        ""Bachelor of Computer Science (Hons)"",
                        ""Bachelor of Arts in Industrial Training Design (Hons)"",
                        ""Bachelor of Arts in Creative Media (Hons)"",
                        ""Bachelor of Mobile Game Development (Honours)""
                    ]
                }
            }"},
        {"foundation in art courses", @"
            {
                ""Sem 1 Year 1"": [
                    ""Introductory Mathematical Analysis"",
                    ""Principles of Economics"",
                    ""Principles of Accounting"",
                    ""Principles of Marketing"",
                    ""Introduction to Business"",
                    ""English I"",
                    ""Bahasa Kebangsaan""
                ],
                ""Sem 2 Year 1"": [
                    ""Presentation Skills"",
                    ""Business Communication"",
                    ""Introduction to Finance"",
                    ""Introduction to Computer Systems and Applications"",
                    ""English II"",
                    ""Event Management"",
                    ""Introduction to Business Law, Introduction to Multimedia, Introduction to Construction, Introduction to Development, Introduction to Environmental Design, Introduction to Quantity Surveying and Applied Mathematics I""
                ],
                ""Sem 3 Year 1"": [
                    ""Professional Writing"",
                    ""Introduction to Statistics"",
                    ""Introduction to Critical Thinking""
                ]
            }"},
            {"foundation in science courses", @"
            {
                ""Sem 1 Year 1"": [
                    ""Mathematics I"",
                    ""Physics I"",
                    ""Chemistry I"",
                    ""English I"",
                    ""Introduction to Critical Thinking"",
                    ""Event Management"",
                    ""Bahasa Kebangsaan""
                ],
                ""Sem 2 Year 1"": [
                    ""Mathematics II"",
                    ""Physics II"",
                    ""Physics Laboratory"",
                    ""Chemistry II"",
                    ""Chemistry Laboratory"",
                    ""English II""
                ],
                ""Sem 3 Year 1"": [
                    ""Mathematics III"",
                    ""Physics III"",
                    ""Computing""
                ]
            }"},
            {"credit hours of foundation", @"
            {
                ""total credit hours"": 50,
                ""semester 1"": 20,
                ""semester 2"": 20,
                ""semester3"": 10
            }"},

            {
            "grade level of foundation", @"
            Yes, this is the structure of grade level in a foundation course.
            Mark         - Grade - Point Value -      Status
            90 -100    -    A+    -   4.00          -   Excellent
            80 - 89     -    A      -   4.00           -   Excellent
            75 - 79      -    A-    -    3.67           -   Excellent 
            70 - 74      -    B+   -    3.33           -   Credit  
            65 - 69      -    B     -    3.00          -   Credit 
            60 - 64     -     B-   -    2.67           -   Credit 
            55 - 59      -    C+   -    2.33           -   Pass
            50 - 54      -    C     -    2.00          -   Pass 
            0 - 49        -     F     -   0.00          -   FAIL

            The general passing grade in all courses is ‘C’ and above."
        },
        {
            "grade point average of foundation",@"
            The Grade Point Average (GPA) is defined as the total grade point received by a student in a semester divided by the number of credit hours/ credit counted in the semester. 
            The formula of GPA is :
            Semester Grade Point / (Total Credit Hours or Credit calculated for that semester)
            The Cumulative Grade Point Average (CGPA) is defined as the sum of all the semesters’ grade points divided by the total credit hours counted for all semesters. 
            The formula of CGPA is :
            Total Semester Grade Points for All Semester / (Total Credit Hours or Credit calculated for all semester)"
        },
        {
            "academic year of foundation",@"
            The Foundation Programme academic year is divided into three semesters - Semester 1, Semester 2 and Semester 3. 
            Semester 1 :
            Classes - 14 Weeks
            Study Week - 1 Week
            Examination - 2 Weeks
            Total - 17 Weeks
            Semester 2 :
            Classes - 14 Weeks
            Study Week - 1 Week
            Examination - 2 Weeks
            Total - 17 Weeks
            Semester 3 :
            Classes - 7 Weeks
            Study Week - 1 Week
            Examination - 1 Weeks
            Total - 9 Weeks"
            },
            {"structure of bachelor of business administration", @"
{
    ""Year 1 Semester 1"": [
        ""Principles of Management"",
        ""Principles of Accounting"",
        ""Principles of Marketing"",
        ""Business Mathematics"",
        ""Business Ethics"",
        ""Management Information Systems"",
        ""Co-curriculum"",
        ""Total: 21 credit""
    ],
    ""Year 1 Semester 2"": [
        ""Organizational Behaviour"",
        ""Introduction to E-Commerce"",
        ""Principles of Finance"",
        ""Microeconomics"",
        ""Business Communication"",
        ""Elective 1"",
        ""Total: 18 credit""
    ],
    ""Year 1 Semester 3 (Short Semester)"": [
        ""Entrepreneurship Skills"",
        ""Appreciation of Ethics and Civilisations / B.Melayu Komunikasi"",
        ""Integrity and Anti-Corruption"",
        ""University Compulsory Subject 1"",
        ""University Compulsory Subject 2"",
        ""Total: 10 credit""
    ],
    ""Year 2 Semester 1"": [
        ""Human Resource Management"",
        ""Financial Accounting"",
        ""Business Statistics"",
        ""Macroeconomics"",
        ""Business Law"",
        ""Elective 2"",
        ""Total: 18 credit""
    ],
    ""Year 2 Semester 2"": [
        ""Cost Accounting"",
        ""Operations Management"",
        ""Project Management"",
        ""Managing Cultural Diversity"",
        ""Final Year Project 1"",
        ""Elective 3"",
        ""Total: 18 credit""
    ],
    ""Year 2 Semester 3 (Short Semester)"": [
        ""Philosophy and Current Issues"",
        ""University Compulsory Subject 3"",
        ""University Compulsory Subject 4"",
        ""University Compulsory Subject 5"",
        ""Total: 10 credit""
    ],
    ""Year 3 Semester 1"": [
        ""Productivity & Quality Management"",
        ""Financial Management"",
        ""Strategic Management"",
        ""International Business"",
        ""Final Year Project 2"",
        ""Elective 4"",
        ""Total: 18 credit""
    ],
    ""Year 3 Semester 2"": [
        ""Industrial Training and Reporting"",
        ""Total: 10 credit""
    ],
    ""Aim"": [
        ""To produce graduates who are successfully engaged in multidisciplinary practices in the business field."",
        ""To produce graduates who are able to demonstrate leadership capabilities in the work environment."",
        ""To produce graduates who are able to demonstrate effective communication in the business environment.""
    ],
    ""Entry Requirement"": [
        ""Pass STPM/ A-level with full pass in two (2) subjects or CGPA 2.0; or"",
        ""Pass UEC with five (5) Grade B’s; or"",
        ""Pass Matriculation/Pre-U/ Foundation from recognised institutions with minimum CGPA 2.0; or"",
        ""Pass Diploma in related field from recognised institutions with minimum CGPA 2.0; or"",
        ""Diploma Vocational Malaysia (DVM) equivalent to credits in any 3 subjects SPM level; and pass with credit in Bahasa Melayu 1104; and minimum academic CGPA of 2.00 out of 4.00; and minimum vocational CGPA of 2.67 out of 4.00; and competence in all vocational modules and for SVM cohort 2012-2016 pass in History; or Other equivalent qualifications recognitions recognised by the Malaysian Government."",
        ""Pass Diploma Kemahiran Malaysia (DKM) / Diploma Lanjutan Kemahiran Malaysia (DLKM) / Diploma Vokasional Malaysia (DVM) with a minimum CGPA of 2.5 subject to UTS Senate approval; or"",
        ""Pass DKM / DLKM / DVM with a minimum CGPA of 2.00 AND have at least 2 years of work experience in a related field.""
    ]
}"
        },
            {"structure of bachelor of accountancy", @"
            {
                ""Year 1 Semester 1"": [
                    ""Management & Practices"",
                    ""Introductory Financial Accounting"",
                    ""Business Law"",
                    ""Microeconomic"",
                    ""Quantitative Methods and Business Decisions"",
                    ""Total: 15 credit""
                ],
                ""Year 1 Semester 2"": [
                    ""Intermediate Financial Accounting and Reporting I"",
                    ""Introductory Management Accounting and Control"",
                    ""Organization Behavior"",
                    ""Accounting Information System"",
                    ""Financial Management"",
                    ""Macroeconomics"",
                    ""Total: 18 credit""
                ],
                ""Year 1 Semester 3 (Short Semester)"": [
                    ""Appreciation of Ethics and Civilizations (Local Student)/ Bahasa Melayu Komunikasi 2 (International Student)"",
                    ""Philosophy and Current Issues (Local Student & International Student)"",
                    ""Co-curriculum"",
                    ""University Compulsory Subject 1"",
                    ""University Compulsory Subject 2 (Bahasa Kebangsaan)"",
                    ""Total: 10 credit""
                ],
                ""Year 2 Semester 1"": [
                    ""Intermediate Financial Accounting and Reporting I"",
                    ""Intermediate Management Accounting and Control"",
                    ""Corporate Finance"",
                    ""Company Law"",
                    ""Analysis and Design of Accounting Information"",
                    ""Introduction to Taxation"",
                    ""Total: 18 credit""
                ],
                ""Year 2 Semester 2"": [
                    ""Advanced Management Accounting and Control"",
                    ""Advanced Financial Accounting and Reporting I"",
                    ""Auditing and Assurance I"",
                    ""Financial Market and Institutions"",
                    ""Intermediate Taxation"",
                    ""Elective"",
                    ""Total: 18 credit""
                ],
                ""Year 2 Semester 3 (Short Semester)"": [
                    ""Malaysia Economy"",
                    ""Entrepreneurial Skills"",
                    ""University Compulsory Subject 3"",
                    ""University Compulsory Subject 4"",
                    ""University Compulsory Subject 5"",
                    ""Total: 10 credit""
                ],
                ""Year 3 Semester 1"": [
                    ""Advanced Financial Accounting and Reporting II"",
                    ""Advanced Taxation"",
                    ""Auditing and Assurance II"",
                    ""Strategic Management"",
                    ""Public Sector Accounting"",
                    ""Elective"",
                    ""Total: 18 credit""
                ],
                ""Year 3 Semester 2"": [
                    ""Industrial Training & Reporting"",
                    ""Total: 10 credit""
                ],
                ""Year 3 Semester 3"": [
                    ""Corporate Governance & Ethics"",
                    ""Integrated Case Studies"",
                    ""Elective"",
                    ""Total: 9 credit""
                ],
                ""Elective Courses"": [
                    ""Green Accounting and Reporting"",
                    ""Islamic Accounting Practices"",
                    ""Forensic Accounting and Fraud Examination"",
                    ""Investment Analysis"",
                    ""International Analysis"",
                    ""Principles of Marketing"",
                    ""e-Commerce""
                ]
            }"},
            {"structure of bachelor of business in marketing", @"
{
    ""Year 1 Semester 1"": [
        ""Principles of Management"",
        ""Principles of Accounting"",
        ""Principles of Marketing"",
        ""Human Resource Management"",
        ""Business Communication"",
        ""Business Economics (Micro & Macro)"",
        ""Co-curriculum""
    ],
    ""Year 1 Semester 2"": [
        ""Organizational Behaviour"",
        ""Consumer Behavior"",
        ""Principles of Finance"",
        ""E-Commerce"",
        ""Applied Statistics"",
        ""Business Law"",
        ""Supervisory Skills""
    ],
    ""Year 1 Semester 3"": [
        ""Entrepreneurship skill"",
        ""Appreciation of Ethics and Civilisations (Penghayatan Etika dan Peradaban) (LS)/B.Melayu Komunikasi 2 (IS)"",
        ""Malaysian Economy"",
        ""University Compulsory Subject 1"",
        ""University Compulsory Subject 2""
    ],
    ""Year 2 Semester 1"": [
        ""Information Marketing"",
        ""Sales Management"",
        ""Social Management"",
        ""Strategic Marketing Management"",
        ""Marketing Research"",
        ""Elective 1""
    ],
    ""Year 2 Semester 2"": [
        ""Managing Cultural Diversity"",
        ""Integrated Marketing Communication"",
        ""Business Ethics"",
        ""Public Relations"",
        ""Final Year Project 1"",
        ""Elective 2""
    ],
    ""Year 2 Semester 3"": [
        ""Philosophy and Current Issues (Falsafah dan isu Semasa)"",
        ""University Compulsory Subject 3"",
        ""University Compulsory Subject 4"",
        ""University Compulsory Subject 5""
    ],
    ""Year 3 Semester 1"": [
        ""Brand Management"",
        ""International Marketing"",
        ""Retailing"",
        ""Final Year Project 2"",
        ""Elective 3"",
        ""Elective 4""
    ],
    ""Year 3 Semester 2"": [
        ""Industrial Training and Reporting""
    ],
    ""Elective Courses"": [
        ""Product Management"",
        ""International Marketing"",
        ""Services Marketing"",
        ""Social Media Marketing"",
        ""Leadership"",
        ""Occupational Safety and Health Management""
    ]
}"},
            {"structure of bachelor of technology management", @"
{
    ""Year 1 Semester 1"": [
        ""Principles Of Management"",
        ""Financial Accounting"",
        ""Organizational Behaviour"",
        ""Technology Management"",
        ""Workshop Technology"",
        ""Digital Electronic""
    ],
    ""Year 1 Semester 2"": [
        ""Human Resource Management"",
        ""Supply Chain Management"",
        ""Data Mining"",
        ""Business Mathematics"",
        ""Computer Aided Design"",
        ""Co-curriculum""
    ],
    ""Year 1 Semester 3"": [
        ""TITAS"",
        ""Green Technology"",
        ""Professional English"",
        ""Creativity and Innovation"",
        ""Bahasa Kebangsaan (Audit Course)""
    ],
    ""Year 2 Semester 1"": [
        ""Hubungan Etnik"",
        ""Project Management"",
        ""Leadership"",
        ""Risk Management"",
        ""Technology Assessment"",
        ""Production Planning and Control""
    ],
    ""Year 2 Semester 2"": [
        ""Lean Enterprise"",
        ""International Business"",
        ""Procurement in Industrial Management"",
        ""Multimedia technology"",
        ""Elective 1"",
        ""Final Project I""
    ],
    ""Year 2 Semester 3"": [
        ""Quality Management"",
        ""Entrepreneurship Skills"",
        ""Malaysian Economy"",
        ""Communication In Workplace""
    ],
    ""Year 3 Semester 1"": [
        ""Vocational Training Operation"",
        ""Training Design, Delivery and Evaluation"",
        ""Elective 2"",
        ""Strategic Management"",
        ""Manufacturing Technology"",
        ""Final Year Project II""
    ],
    ""Year 3 Semester 2"": [
        ""Industrial Training""
    ],
    ""Elective Courses"": [
        ""Marketing Management"",
        ""Industrial Management"",
        ""Principle of Finance""
    ]
}"},
            {"structure of bachelor of mechanical engineering", @"
{
    ""Year 1 Semester 1"": [
        ""Statics"",
        ""Material Science"",
        ""Engineering Laboratory 1"",
        ""Engineering Graphics"",
        ""Mathematics I"",
        ""University Compulsory Subject 1"",
        ""Appreciation of Ethics and Civilisations"",
        ""Co-curriculum""
    ],
    ""Year 1 Semester 2"": [
        ""Dynamics"",
        ""Mechanics of Materials"",
        ""Electrical Engineering"",
        ""Engineering Laboratory 2"",
        ""Engineering Workshop"",
        ""Engineering Programming"",
        ""Mathematics II"",
        ""Philosophy and Current Issues""
    ],
    ""Year 2 Semester 1"": [
        ""Electronic Engineering"",
        ""Thermodynamics I"",
        ""Fluid Mechanics I"",
        ""Measurement and Instrumentation Systems"",
        ""Engineering Laboratory 3"",
        ""Mathematics 3"",
        ""University Compulsory Subject 2""
    ],
    ""Year 2 Semester 2"": [
        ""Material Engineering"",
        ""ThermodynamicsII"",
        ""Fluid Mechanics II"",
        ""Product Design"",
        ""Computer Aided Design"",
        ""Engineering Laboratory 4"",
        ""Engineering Statistics"",
        ""Entrepreneurship Skills""
    ],
    ""Year 3 Semester 1"": [
        ""Mechanics of Machines"",
        ""Heat Transfer"",
        ""Mechanical Design"",
        ""Control Engineering"",
        ""Numerical Methods for Engineers"",
        ""Basic Accounting and Finance""
    ],
    ""Year 3 Semester 2"": [
        ""Mechanical Vibration"",
        ""Engineering Ethics"",
        ""Industrial Safety and Health"",
        ""Integrated Design Project"",
        ""University Compulsory Subject 3"",
        ""University Compulsory 4""
    ],
    ""Year 3 Semester 3"": [
        ""Industrial Training""
    ],
    ""Year 4 Semester 1"": [
        ""Manufacturing Technology"",
        ""Technical Elective 1"",
        ""Technical Elective 2"",
        ""Final Year Project I"",
        ""University Compulsory Subject 5""
    ],
    ""Year 4 Semester 2"": [
        ""Project Management"",
        ""Professional Engineer"",
        ""Technical Elective 3"",
        ""Technical Elective 4"",
        ""Final Year Project II""
    ],
    ""Technical Electives"": [
        ""Robotics"",
        ""Advanced Manufacturing"",
        ""Automotive Engineering"",
        ""Renewable Energy""
    ]
}"},
            {
    "structure of bachelor of civil engineering",@"
{
    ""Year 1 Semester 1"": [
        ""Engineering Mechanics"",
        ""Mathematics I"",
        ""Appreciation of Ethics and Civilisations"",
        ""Building Services"",
        ""Engineering Graphics"",
        ""University Compulsory Subject 1"",
        ""Co-curriculum""
    ],
    ""Year 1 Semester 2"": [
        ""Civil Engineering Materials"",
        ""Strength of Materials"",
        ""Engineering Surveying"",
        ""Mathematics II"",
        ""Philosophy and Current Issues"",
        ""Engineering Programming""
    ],
    ""Year 1 Semester 3"": [
        ""Survey Camp""
    ],
    ""Year 2 Semester 1"": [
        ""Construction Technology"",
        ""Theory of Structure"",
        ""Fluid Mechanics"",
        ""Mathematics III"",
        ""University Compulsory Subject 2"",
        ""Civil Engineering Lab I"",
        ""Highway Engineering I""
    ],
    ""Year 2 Semester 2"": [
        ""Structural Analysis"",
        ""Soil Mechanics"",
        ""Hydraulics"",
        ""Highway Engineering II"",
        ""Engineering Statistics"",
        ""Entrepreneurship Skills / Bahasa Kebangsaan A"",
        ""Civil Engineering Lab II""
    ],
    ""Year 3 Semester 1"": [
        ""Water Supply & Wastewater Engineering"",
        ""Geotechnical Engineering"",
        ""Reinforced Concrete Design I"",
        ""Hydrology"",
        ""Basic Accounting & Finance"",
        ""Civil Engineering Lab III"",
        ""Integrity and Anti Corruption""
    ],
    ""Year 3 Semester 2"": [
        ""Environmental Engineering"",
        ""Reinforced Concrete Design II"",
        ""Traffic Engineering"",
        ""Structural Steel Design"",
        ""University Compulsory Subject 3"",
        ""University Compulsory Subject 4""
    ],
    ""Year 3 Semester 3"": [
        ""Industrial training""
    ],
    ""Year 4 Semester 1"": [
        ""Integrated Design Project I"",
        ""Final Project II"",
        ""Technical Elective I"",
        ""Construction Management"",
        ""Estimating & Contract""
    ],
    ""Year 4 Semester 2"": [
        ""Integrated Design Project II"",
        ""Final Year Project II"",
        ""Technical Elective II"",
        ""Engineering Ethics"",
        ""University Compulsory Subject 5""
    ],
    ""Elective Courses"": [
        ""Foundation Enginnering"",
        ""Project Planning and Control"",
        ""Project Management Professional"",
        ""Software Aides Structural Design"",
        ""Pre-stressed Concrete Design"",
        ""Timber Design"",
        ""Concrete Technology""
    ]
}"},
        };

        public string GetAnswer(string question)
        {
            string normalizedQuestion = question.ToLower().Trim();
            if (jsonData.TryGetValue(normalizedQuestion, out string output))
            {
                return output;
            }
            return "Question not found.";
        }

        public string GetRelevantData(string message)
        {
            var keywords = new List<string> { "foundation program", "undergraduate", "postgraduate","program", "foundation in art courses"," foundation in science courses","credit hours of foundation", "grade level of foundation", "grade point average of foundation","academic year of foundation", "structure of bachelor of business administration","structure of bachelor of accountancy","structure of bachelor of businees in marketing","structure of bachelor of technology management","structure of bachelor of mechanical engineering", "structure of bachelor of civil engineering" };
            var relevantData = new List<string>();

            foreach (var keyword in keywords)
            {
                if (message.ToLower().Contains(keyword))
                {
                    foreach (var entry in jsonData)
                    {
                        if (entry.Key.Contains(keyword))
                        {
                            relevantData.Add($"{entry.Key}: {entry.Value}");
                        }
                    }
                }
            }

            return relevantData.Count > 0 ? string.Join(", ", relevantData) : string.Empty;
        }
    }
}








