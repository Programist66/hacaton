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
                }

            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
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
            public Image(int id, string filename, byte[] data)
            {
                Id = id;
                FileName = filename;
                Data = data;
            }
            public int Id { get; private set; }
            public string FileName { get; private set; }
            public byte[] Data { get; private set; }

            public void SaveOnDevice(string path) 
            {
                Bitmap temp = ConvertBytesToImage(this.Data);
                temp.Save(path + "\\" + this.FileName, System.Drawing.Imaging.ImageFormat.Png);
                WriteLine($"Файл скачан по пути: {path + "\\" + this.FileName}");
            }
        }


        

        public void LoadPhotoToBD(string path)
        {
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
                return;
            }
            connection.Close();
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
                }

            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                return;
            }
            connection.Close();
        }

        public class DocumentType 
        {
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

        

        public void LoadDocumentToBD(string path)
        {
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
                return;
            }
            connection.Close();
        }

        public DocumentType[] DownloadDocumentFromBD(string ID = "")
        {
            List<DocumentType> documentTypes = new List<DocumentType>();
            try
            {
                connection.Open();
                try
                {
                    if(ID != "") 
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
        private BDCardControl() { }
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
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                return;
            }
            connection.Close();
        }
    }
    class CardObject 
    {

    }
    class Interface
    {

    }
}


namespace hacaton
{
    internal class Program
    {
        static string pathToBD = "server=localhost;user=root;password=Rotter;database=user";

        static void Main(string[] args)
        {

            BDFileWorker.Photo PhotoWorker = new BDFileWorker.Photo(pathToBD);
            BDFileWorker.Document DocumentWorker = new BDFileWorker.Document(pathToBD);
            MainCardObject.BDCardControl BDCard = new MainCardObject.BDCardControl(pathToBD);

            int choise = 0;
            do
            {
                Write("[0] - Выход\n[1] - Загрузить фото в карточку\n[2] - Скачать фото из базы данных\n[3] - Загрузить документ в карточку\n[4] - Скачать документ из базы данных\nВаш выбор: ");
                Int32.TryParse(ReadLine(), out choise);
                if (choise == 1)
                {
                    Write("Введите путь до фото: ");
                    string path = ReadLine();
                    PhotoWorker.LoadPhotoToBD(path);
                }
                if (choise == 2)
                { 
                    Write("Введите путь куда сохранить фото: ");
                    string path = ReadLine();
                    BDFileWorker.Photo.Image[] imgs = PhotoWorker.DownloadPhotoFromBD();
                    foreach (var item in imgs)
                    {
                        item.SaveOnDevice(path);
                    }
                }
                if (choise == 3)
                {
                    Write("Введите путь до документа: ");
                    string path = ReadLine();
                    DocumentWorker.LoadDocumentToBD(path);
                }
                if (choise == 4)
                {
                    Write("Введите путь куда сохранить документ: ");
                    string path = ReadLine();
                    BDFileWorker.Document.DocumentType[] docs = DocumentWorker.DownloadDocumentFromBD();
                    foreach (var item in docs)
                    {
                        item.SaveOnDevice(path);
                    }
                }
                if (choise < 0 || choise > 4)
                {
                    WriteLine("Неверный выбор!!!");
                }
            }while (choise != 0);

        }
    }
}
