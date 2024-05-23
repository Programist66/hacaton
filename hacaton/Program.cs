using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.IO;
using System.Xml.Linq;
using MainCardObject;

namespace BDFileWorker
{

    class Photo
    {
        static MySqlConnection connection;
        static MySqlCommand command;

        private Photo() { }
        public Photo(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                try
                {
                    string query = "CREATE TABLE IF NOT EXISTS Photo (id INT AUTO_INCREMENT PRIMARY KEY,image_title VARCHAR(255),image_data LONGBLOB)";
                    command = new MySqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine("Таблица создана или существуюет.");
                    ResetColor();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка создания таблицы." + ex.Message);
                    ResetColor();
                }

            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                ResetColor();
                return;
            }
            connection.Close();
        }
        static public Bitmap ConvertBytesToImage(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                return new Bitmap(ms);
            }
        }

        public class Image
        {
            public Image() { }
            public Image(int id, string filename, byte[] data)
            {
                Id = id;
                FileName = filename;
                Data = data;
            }
            public int Id { get; private set; }
            public string FileName { get; private set; }
            public byte[] Data { get; private set; }

            public void SaveOnDevice(string path = "")
            {
                Bitmap temp = ConvertBytesToImage(this.Data);
                temp.Save(path + "\\" + this.FileName, System.Drawing.Imaging.ImageFormat.Png);
                WriteLine($"Файл скачан по пути: {path + "\\" + this.FileName}");
            }
        }




        public Image LoadPhotoToBD(string path)
        {
            Image img = new Image();
            string shortFileName = path.Substring(path.LastIndexOf('\\') + 1);
            try
            {
                connection.Open();
                try
                {
                    string query = "INSERT INTO Photo (image_title,image_data) VALUES (@image_title, @image_data)";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@image_title", shortFileName);
                    command.Parameters.AddWithValue("@image_data", File.ReadAllBytes(path));
                    command.ExecuteNonQuery();
                    WriteLine("фото добавлено");                    
                    query = "SELECT id FROM Photo ORDER BY id DESC LIMIT 1";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    img = new Image(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при добавлние фота" + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
            }
            connection.Close();
            return img;
        }

        public Image[] DownloadPhotoFromBD(string ID = "")
        {
            List<Image> images = new List<Image>();
            try
            {
                connection.Open();
                try
                {
                    if (ID != "")
                    {
                        ID = $"where ID = {ID}";
                    }
                    string query = $"SELECT * FROM Photo {ID}";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string file = reader.GetString(1);
                        byte[] data = (byte[])reader.GetValue(2);
                        images.Add(new Image(id, file, data));
                    }

                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при скачивание фота" + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
            }
            connection.Close();
            return images.ToArray();
        }
    }

    class Document
    {
        static MySqlConnection connection;
        static MySqlCommand command;

        private Document() { }
        public Document(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                try
                {
                    string query = "CREATE TABLE IF NOT EXISTS Documents (id INT AUTO_INCREMENT PRIMARY KEY,document_title VARCHAR(255),document_data LONGBLOB)";
                    command = new MySqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine("Таблица создана или существуюет.");
                    ResetColor();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка создания таблицы." + ex.Message);
                    ResetColor();
                }

            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                ResetColor();
                return;
            }
            connection.Close();
        }

        public class DocumentType
        {
            public DocumentType() { }
            public DocumentType(int id, string filename, byte[] data)
            {
                Id = id;
                FileName = filename;
                Data = data;
            }
            public int Id { get; private set; }
            public string FileName { get; private set; }
            public byte[] Data { get; private set; }
            public void SaveOnDevice(string outputPath)
            {
                File.WriteAllBytes(outputPath + "\\" + FileName, this.Data);
                WriteLine($"Файл скачан по пути: {outputPath + '\\' + FileName}");
            }
        }



        public DocumentType LoadDocumentToBD(string path)
        {
            DocumentType documentType = new DocumentType();
            string shortFileName = path.Substring(path.LastIndexOf('\\') + 1);
            try
            {
                connection.Open();
                try
                {
                    string query = "INSERT INTO Documents (document_title,document_data) VALUES (@document_title, @document_data)";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@document_title", shortFileName);
                    command.Parameters.AddWithValue("@document_data", File.ReadAllBytes(path));
                    command.ExecuteNonQuery();
                    WriteLine("документ добавлен");
                    query = "SELECT * FROM item ORDER BY id DESC LIMIT 1";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    documentType = new DocumentType(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при добавлние документа" + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
            }
            connection.Close();
            return documentType;
        }

        public DocumentType[] DownloadDocumentFromBD(string ID = "")
        {
            List<DocumentType> documentTypes = new List<DocumentType>();
            try
            {
                connection.Open();
                try
                {
                    if (ID != "")
                    {
                        ID = $"where ID = {ID}";
                    }
                    string query = $"SELECT * FROM Documents {ID}";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    DocumentType doc;
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string file = reader.GetString(1);
                        byte[] data = (byte[])reader.GetValue(2);
                        documentTypes.Add(new DocumentType(id, file, data));
                    }
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при скачивание фота" + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
            }
            connection.Close();
            return documentTypes.ToArray();
        }
    }
}

namespace MainCardObject
{
    class BDCardControl
    {
        static MySqlConnection connection;
        static MySqlCommand command;
        public class BDCart 
        {
            int id;
            public string nameObject { get; }
            public string adress { get; }
            public string cadastralNumber { get; }
            public int countFloors { get; }
            public float square { get; }
            public string photos { get; }
            public DateTime dateOfBuilding { get; }
            public string documents { get; }
            public string works { get; }
            public string customer { get; }
            public string executor { get; }
            public DateTime dateOfRegistration { get; }
            public DateTime completionDate { get; }
            public string department { get; }

            public BDCart(CardObject cart)
            {
                nameObject = cart.nameObject;
                adress = cart.adress;
                cadastralNumber = cart.cadastralNumber;
                countFloors = cart.countFloors;
                square = cart.square;
                if (cart.photos[0] != null)
                {
                    photos = (cart.photos[0].Id).ToString();
                    foreach (var item in cart.photos)
                    {
                        if (item.Id != cart.photos[0].Id)
                        {
                            photos += "," + item.Id;
                        }
                    }
                }
                dateOfBuilding = cart.dateOfBuilding;
                if (cart.documents[0] != null)
                {
                    documents = (cart.documents[0].Id).ToString();
                    foreach (var item in cart.documents)
                    {
                        if (item.Id != cart.documents[0].Id)
                        {
                            documents += "," + item.Id;
                        }
                    }
                }
                works = cart.works;
                customer = cart.customer;
                executor = cart.executor;
                dateOfRegistration = cart.dateOfRegistration;
                completionDate = cart.completionDate;
                department = cart.department;
            }

        }
        public BDCardControl(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                try
                {
                    string query = "CREATE TABLE IF NOT EXISTS CardObject (id INT PRIMARY KEY AUTO_INCREMENT," +
                                                                            "NameObject TEXT NOT NULL," +
                                                                            "Adress TEXT NOT NULL," +
                                                                            "CadastralNumber VARCHAR(15) NOT NULL," +
                                                                            "CountFloors TINYINT," +
                                                                            "Square Float NOT NULL," +
                                                                            "Photo TINYTEXT," +
                                                                            "DateBuilding Date NOT NULL," +
                                                                            "Documents TINYTEXT NOT NULL," +
                                                                            "Works TEXT NOT NULL," +
                                                                            "Customer TEXT NOT NULL," +
                                                                            "Executor TEXT NOT NULL," +
                                                                            "DateRegistration Date NOT NULL," +
                                                                            "CompletionDate Date NOT NULL," +
                                                                            "Department TEXT NOT NULL)";
                    command = new MySqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine("Таблица создана или существуюет.");
                    ResetColor();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка создания таблицы." + ex.Message);
                    ResetColor();
                }

            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                ResetColor();
                return;
                
            }
            connection.Close();
        }

        public void AddCard(BDCart Cart) 
        {
            string query = "INSERT INTO Photo (NameObject," +
                                                "Adress," +
                                                "CadastralNumber," +
                                                "CountFloors," +
                                                "Square," +
                                                "Photo, " +
                                                "DateBuilding, " +
                                                "Documents," +
                                                "Works, " +
                                                "Customer, " +
                                                "Executor," +
                                                "DateRegistration, " +
                                                "CompletionDate," +
                                                "Department) VALUES " +
                                                "(@NameObject," +
                                                "@Adress," +
                                                "@CadastralNumber," +
                                                "@CountFloors," +
                                                "@Square," +
                                                "@Photo, " +
                                                "@DateBuilding, " +
                                                "@Documents," +
                                                "@Works, " +
                                                "@Customer, " +
                                                "@Executor," +
                                                "@DateRegistration, " +
                                                "@CompletionDate," +
                                                "@Department)";
            command = new MySqlCommand(query,connection);
            command.Parameters.AddWithValue("@NameObject", Cart.nameObject);
            command.Parameters.AddWithValue("@CadastralNumber", Cart.cadastralNumber);
            command.Parameters.AddWithValue("@CountFloors", Cart.countFloors);
            command.Parameters.AddWithValue("@Square", Cart.square);
            command.Parameters.AddWithValue("@Photo", Cart.photos);
            command.Parameters.AddWithValue("@DateBuilding", Cart.dateOfBuilding);
            command.Parameters.AddWithValue("@Documents", Cart.documents);
            command.Parameters.AddWithValue("@Works", Cart.works);
            command.Parameters.AddWithValue("@Customer", Cart.customer);
            command.Parameters.AddWithValue("@Executor", Cart.executor);
            command.Parameters.AddWithValue("@DateRegistration", Cart.dateOfRegistration);
            command.Parameters.AddWithValue("@CompletionDate", Cart.completionDate);
            command.Parameters.AddWithValue("@Department", Cart.department);
            command.ExecuteNonQuery();
        }
    }
    class CardObject
    {
        public int id { get; set; }
        public string nameObject { get; set; }
        public string adress { get; set; }
        public string cadastralNumber { get; set; }
        public int countFloors { get; set; }
        public float square { get; set; }
        public List<BDFileWorker.Photo.Image> photos { get; set; }
        public DateTime dateOfBuilding { get; set; }
        public List<BDFileWorker.Document.DocumentType> documents { get; set; }
        public string works { get; set; }
        public string customer { get; set; }
        public string executor { get; set; }
        public DateTime dateOfRegistration { get; set; }
        public DateTime completionDate { get; set; }
        public string department { get; set; }

       CardObject(BDCardControl.BDCart bDCart)
        {

        }

       public CardObject(string nameObject, string adress, string cadastralNumber, int countFloors, double square, List<BDFileWorker.Photo.Image> photos, DateTime dateOfBuilding, List<BDFileWorker.Document.DocumentType> documents, string works, string customer, string executor, DateTime dateOfRegistration, DateTime completionDate, string department) 
        {
            this.nameObject = nameObject;
            this.adress = adress;
            this.cadastralNumber = cadastralNumber;
            this.countFloors = countFloors;
            this.square = (float)square;
            this.photos = photos;
            this.dateOfBuilding = dateOfBuilding;
            this.documents = documents;
            this.works = works;
            this.customer = customer;
            this.executor = executor;
            this.dateOfRegistration = dateOfRegistration;
            this.completionDate = completionDate;
            this.department = department;
        }
    }
    class Interface
    {
        BDCardControl BD;
        BDFileWorker.Photo PhotoWorker;
        BDFileWorker.Document DocumentWorker;
        public Interface(BDCardControl bd, string pathToBD) 
        {
            BD = bd;
            PhotoWorker = new BDFileWorker.Photo(pathToBD);
            DocumentWorker = new BDFileWorker.Document(pathToBD);
        }

        public void DrawMainmenu() 
        {
            int choise = 0;            
            do 
            {
                Clear();
                Console.Write("[0] - Выход\n[1] - Создать карточку\n[2] - Посмотреть карточку\n[3] - Удалить карточку\n[4] - Загрузить XML карточку\n[5] - скачать XML карточку\nВаш выбор: ");
                Int32.TryParse(ReadLine(), out choise);
                switch ( choise ) 
                {
                    case 0:
                        Console.WriteLine();
                        break;
                    case 1:
                        List<BDFileWorker.Photo.Image> imgs = new List<BDFileWorker.Photo.Image>();
                        imgs.Add(PhotoWorker.LoadPhotoToBD(@"C:\Users\Student\Downloads\63fe2f690596f.png"));
                        List<BDFileWorker.Document.DocumentType> docs = new List<BDFileWorker.Document.DocumentType>();
                        docs.Add(DocumentWorker.LoadDocumentToBD(@"C:\Users\Student\Downloads\Что такое ндфл.pptx"));
                        BD.AddCard(new CardObject("Арбайтен", "город Пушкино, дом калутушкино", "1512412427", 56, 21458.1, imgs, new DateTime(15,01,2021), docs, "застрелиться", "Егор", "Ваня", new DateTime(10,02,2020), new DateTime(11,03,2022), "Академия") as CardObject);
                        Console.WriteLine();
                        break;
                    case 2:
                        Console.WriteLine();
                        break;
                    case 3:
                        Console.WriteLine();
                        break;
                    case 4:
                        Console.WriteLine();
                        break;
                    case 5:
                        Console.WriteLine();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор!!");
                        break;
                }
            } while (choise != 0);
        }
    }
}


namespace hacaton
{
    internal class Program
    {
        static string pathToBD = "server=localhost;user=root;password=1234;database=Users1";

        static void Main(string[] args)
        {


            MainCardObject.BDCardControl BDCard = new BDCardControl(pathToBD);
            MainCardObject.Interface inter = new Interface(BDCard, pathToBD);



            //    int choise = 0;
            //    do
            //    {
            //        Clear();
            //        Write("[0] - Выход\n[1] - Загрузить фото в карточку\n[2] - Скачать фото из базы данных\n[3] - Загрузить документ в карточку\n[4] - Скачать документ из базы данных\nВаш выбор: ");
            //        Int32.TryParse(ReadLine(), out choise);
            //        if (choise == 1)
            //        {
            //            Write("Введите путь до фото: ");
            //            string path = ReadLine();
            //            PhotoWorker.LoadPhotoToBD(path);
            //        }
            //        if (choise == 2)
            //        {
            //            Write("Введите путь куда сохранить фото: ");
            //            string path = ReadLine();
            //            BDFileWorker.Photo.Image[] imgs = PhotoWorker.DownloadPhotoFromBD();
            //            foreach (var item in imgs)
            //            {
            //                item.SaveOnDevice(path);
            //            }
            //        }
            //        if (choise == 3)
            //        {
            //            Write("Введите путь до документа: ");
            //            string path = ReadLine();
            //            DocumentWorker.LoadDocumentToBD(path);
            //        }
            //        if (choise == 4)
            //        {
            //            Write("Введите путь куда сохранить документ: ");
            //            string path = ReadLine();
            //            BDFileWorker.Document.DocumentType[] docs = DocumentWorker.DownloadDocumentFromBD();
            //            foreach (var item in docs)
            //            {
            //                item.SaveOnDevice(path);
            //            }
            //        }
            //        if (choise < 0 || choise > 4)
            //        {
            //            WriteLine("Неверный выбор!!!");
            //        }
            //    } while (choise != 0);

            //}
        }
    }
}
