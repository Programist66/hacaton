using System;
using System.Collections.Generic;
using static System.Console;
using MySql.Data.MySqlClient;
using System.Xml;
using System.Threading;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Drawing;
using System.IO;
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
                    connection.Close();
                    connection.Open();
                    query = "SELECT * FROM Photo ORDER BY id DESC LIMIT 1";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        img = new Image(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    }
                    reader.Close();
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

        public Image DownloadPhotoFromBD(string ID = "")
        {
            Image image = new Image();
            try
            {
                connection.Open();
                try
                {
                    if (ID != "")
                    {
                        ID = $"WHERE id = {ID}";
                    }
                    string query = $"SELECT * FROM Photo {ID}";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        image = new Image(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    }
                    reader.Close();

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
            return image;
        }
    }

    class Document
    {
        static MySqlConnection connection;
        static MySqlCommand command;


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
                    connection.Close();
                    connection.Open();
                    query = "SELECT * FROM Documents ORDER BY id DESC LIMIT 1";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    documentType = new DocumentType();
                    while (reader.Read())
                    {
                        documentType = new DocumentType(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    }
                    reader.Close();
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

        public DocumentType DownloadDocumentFromBD(string ID = "")
        {
            DocumentType documentTypes = new DocumentType();
            try
            {
                connection.Open();
                try
                {
                    if (ID != "")
                    {
                        ID = $"WHERE id = {ID}";
                    }

                    string query = $"SELECT * FROM Documents {ID}";
                    command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        documentTypes = new DocumentType(reader.GetInt32(0), reader.GetString(1), (byte[])reader.GetValue(2));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при скачивание документа" + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
            }
            connection.Close();
            return documentTypes;
        }
    }
}

namespace MainCardObject
{


    class BDCardControl
    {
        public BDFileWorker.Photo PhotoWorker;
        public BDFileWorker.Document DocumentWorker;
        static MySqlConnection connection;
        static MySqlCommand command;
        public class BDCart
        {
            public int id { get; set; }
            public string nameObject { get; set; }
            public string adress { get; set; }
            public string cadastralNumber { get; set; }
            public int countFloors { get; set; }
            public float square { get; set; }
            public string photos { get; set; }
            public DateTime dateOfBuilding { get; set; }
            public string documents { get; set; }
            public string works { get; set; }
            public string customer { get; set; }
            public string executor { get; set; }
            public DateTime dateOfRegistration { get; set; }
            public DateTime completionDate { get; set; }
            public string department { get; set; }

            public BDCart() { }

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
            PhotoWorker = new BDFileWorker.Photo(connectionString);
            DocumentWorker = new BDFileWorker.Document(connectionString);
            try
            {
                connection.Open();
                try
                {
                    string query = "CREATE TABLE IF NOT EXISTS CardObject (id INT PRIMARY KEY AUTO_INCREMENT," +
                                                                            "NameObject TEXT NOT NULL," +
                                                                            "Adress TEXT NOT NULL," +
                                                                            "CadastralNumber VARCHAR(18) NOT NULL," +
                                                                            "CountFloors TINYINT NOT NULL," +
                                                                            "Square Float NOT NULL," +
                                                                            "Photo TINYTEXT NOT NULL," +
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
            try
            {
                connection.Open();
                try
                {
                    string query = "INSERT INTO CardObject (NameObject," +
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
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@NameObject", Cart.nameObject);
                    command.Parameters.AddWithValue("@Adress", Cart.adress);
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
                    connection.Close();
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка создания карточки." + ex.Message);
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

        public BDCart GetCard(string nameObject)
        {
            BDCart card = new BDCart();
            try
            {
                connection.Open();
                try
                {

                    string query = $"SELECT * FROM CardObject WHERE NameObject = @NameObject";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@NameObject", nameObject);
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        card.id = reader.GetInt32("id");
                        card.nameObject = reader.GetString("NameObject");
                        card.adress = reader.GetString("Adress");
                        card.cadastralNumber = reader.GetString("CadastralNumber");
                        card.countFloors = reader.GetInt32("CountFloors");
                        card.square = reader.GetFloat("Square");
                        card.photos = reader.GetString("Photo");
                        card.dateOfBuilding = reader.GetDateTime("DateBuilding");
                        card.documents = reader.GetString("Documents");
                        card.works = reader.GetString("Works");
                        card.customer = reader.GetString("Customer");
                        card.executor = reader.GetString("Executor");
                        card.dateOfRegistration = reader.GetDateTime("DateRegistration");
                        card.completionDate = reader.GetDateTime("CompletionDate");
                        card.department = reader.GetString("Department");
                        connection.Close();
                        return card;
                    }
                    else
                    {
                        connection.Close();
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("Обект не найден!!!");
                        ResetColor();
                        throw new Exception("");
                    }

                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка поиска карточки." + ex.Message);
                    ResetColor();
                }

            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                ResetColor();
            }
            connection.Close();
            return card;
        }

        public bool DeleteCard(string nameObject)
        {
            try
            {
                connection.Open();
                try
                {
                    string query = $"DELETE FROM CardObject WHERE NameObject = @NameObject";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@NameObject", nameObject);
                    int AmountDelete = command.ExecuteNonQuery();
                    if (AmountDelete > 0)
                    {
                        ForegroundColor = ConsoleColor.Green;
                        WriteLine($"Объект \"{nameObject}\" успешно удален");
                        ResetColor();
                        Thread.Sleep(1000);
                        connection.Close();
                        return true;
                    }
                    else
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine($"Не удалось найти объект \"{nameObject}\"");
                        ResetColor();
                        connection.Close();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка удаления карточки: " + ex.Message);
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                ResetColor();
            }
            connection.Close();
            return false;
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

        public CardObject() { }

        public CardObject(BDCardControl.BDCart BdCart, BDCardControl BD)
        {
            photos = new List<BDFileWorker.Photo.Image>();
            documents = new List<BDFileWorker.Document.DocumentType>();
            id = BdCart.id;
            nameObject = BdCart.nameObject;
            adress = BdCart.adress;
            cadastralNumber = BdCart.cadastralNumber;
            countFloors = BdCart.countFloors;
            square = BdCart.square;

            foreach (var item in BdCart.photos.Split(','))
            {
                photos.Add(BD.PhotoWorker.DownloadPhotoFromBD(item));
            }
            dateOfBuilding = BdCart.dateOfBuilding;
            foreach (var item in BdCart.documents.Split(','))
            {
                documents.Add(BD.DocumentWorker.DownloadDocumentFromBD(item));
            }
            works = BdCart.works;
            customer = BdCart.customer;
            executor = BdCart.executor;
            dateOfRegistration = BdCart.dateOfRegistration;
            completionDate = BdCart.completionDate;
            department = BdCart.department;
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

        public override string ToString()
        {
            string s = $"Название объекта: {nameObject}\nАдрес объекта: {adress}\nКадастровый номер объекта: {cadastralNumber}\n{(countFloors == 0 ? "" : $"Колличество этажей: {countFloors}\n")}Площадь объекта: {square}\n";
            s += "Фотографии: \n";
            foreach (var item in photos)
            {
                s += "\t\t" + item.FileName + "\n";
            }
            s += $"Дата постройки объекта: {dateOfBuilding.ToLongDateString()}\n";
            s += "Документы: \n";
            foreach (var item in documents)
            {
                s += "\t\t" + item.FileName + "\n";
            }
            s += $"Работы по объекту: {works}\nЗаказчик: {customer}\nИсполнитель: {executor}\nДата регистрации карточки: {dateOfRegistration.ToLongDateString()}\nДата окончания работ: {completionDate.ToLongDateString()}\nДепартамент: {department}\n";
            return s;
        }

        public static string CadastarlValidation(string Cadastral)
        {
            string[] temp = Cadastral.Split(':');
            if (temp.Length != 4)
            {
                return "";
            }
            else
            {
                foreach (var item in temp)
                {
                    foreach (var item1 in item)
                    {
                        if (!char.IsDigit(item1))
                        {
                            return "";
                        }
                    }
                }
                temp[0] = temp[0].PadLeft(2, '0');
                temp[1] = temp[1].PadLeft(2, '0');
                temp[2] = temp[2].PadLeft(7, '0');
                temp[3] = temp[3].PadLeft(4, '0');
                return string.Join(":", temp);
            }
        }
    }
    class Interface
    {
        BDCardControl BD;
        string pathToBD;


        public Interface(BDCardControl bd, string pathToBD)
        {
            BD = bd;
            this.pathToBD = pathToBD;
        }

        public void AddCardMenu()
        {

            Clear();
            CardObject card = new CardObject();
            //---------------------------
            Console.Write("Введите название объекта: ");
            string temp = Console.ReadLine();
            card.nameObject = temp;
            //---------------------------
            Console.Write("Введите адрес объекта: ");
            temp = Console.ReadLine();
            card.adress = temp;
            //---------------------------
            do
            {
                Console.Write("Введите кадастровый номер: ");
                temp = Console.ReadLine();
                temp = CardObject.CadastarlValidation(temp);
                if (temp == "")
                {
                    Console.WriteLine("Неверно введен кадастровый номер!!");
                }
            } while (temp == "");
            card.cadastralNumber = temp;
            //---------------------------
            int tempI;
            Console.Write("Введите количество этажей(0 - если нельзя определить): ");
            bool flag;
            do
            {
                flag = Int32.TryParse(ReadLine(), out tempI);
                if (!flag && tempI < 0)
                {
                    WriteLine("Некорретные данные!!");
                }
            } while (!flag && tempI < 0);
            card.countFloors = tempI;
            //---------------------------
            float tempF;
            Console.Write("Введите площадь объекта: ");
            do
            {
                flag = float.TryParse(ReadLine(), out tempF);
                if (!flag && tempF < 0)
                {
                    WriteLine("Некорретные данные!!");
                }
            } while (!flag && tempF < 0);
            card.square = tempF;
            //---------------------------
            List<BDFileWorker.Photo.Image> imgs = new List<BDFileWorker.Photo.Image>();
            do
            {
                Write("Введите количесво фотографий(не меньше 1): ");
                flag = Int32.TryParse(ReadLine(), out tempI);
                if (!flag || tempI < 1)
                {
                    WriteLine("Неверное количество!!");
                }
            } while (!flag || tempI < 1);
            for (int i = 0; i < tempI; i++)
            {
                do
                {
                    Write("Введите путь до изображения: ");
                    temp = ReadLine();
                    flag = File.Exists(temp);
                    if (!flag)
                    {
                        WriteLine("Неверный путь или изображение не существует!");
                    }
                } while (!flag);
                imgs.Add(BD.PhotoWorker.LoadPhotoToBD(temp));
            }
            card.photos = imgs;
            //---------------------------
            DateTime tempDate;
            do
            {
                Write("Введите дату постройки объекта (в формате год/месяц/день): ");
                flag = DateTime.TryParse(ReadLine(), out tempDate);
                if (!flag)
                {
                    WriteLine("Неверный формат даты!!");
                }
            } while (!flag);
            card.dateOfBuilding = tempDate;
            //---------------------------
            List<BDFileWorker.Document.DocumentType> docs = new List<BDFileWorker.Document.DocumentType>();
            do
            {
                Write("Введите количесво документов(не меньше 1): ");
                flag = Int32.TryParse(ReadLine(), out tempI);
                if (!flag || tempI < 1)
                {
                    WriteLine("Неверное количество!!");
                }
            } while (!flag || tempI < 1);
            for (int i = 0; i < tempI; i++)
            {
                do
                {
                    Write("Введите путь до документа: ");
                    temp = ReadLine();
                    flag = File.Exists(temp);
                    if (!flag)
                    {
                        WriteLine("Неверный путь или файл не существует!!");
                    }
                } while (!flag);
                docs.Add(BD.DocumentWorker.LoadDocumentToBD(temp));
            }
            card.documents = docs;
            //---------------------------
            Write("Напишите какие работы надо выполнить: ");
            temp = ReadLine();
            card.works = temp;
            //---------------------------
            Write("Введите заказчика(-ов): ");
            temp = ReadLine();
            card.customer = temp;
            //---------------------------
            Write("Введите исполнителя(-ей): ");
            temp = ReadLine();
            card.executor = temp;
            //---------------------------
            card.dateOfRegistration = DateTime.Now;
            //---------------------------
            do
            {
                Write("Введите дату окончания работ по объекту (в формате год/месяц/день): ");
                flag = DateTime.TryParse(ReadLine(), out tempDate);
                if (!flag)
                {
                    WriteLine("Неверный формат даты!!");
                }
                else
                {
                    if (tempDate < card.dateOfBuilding || tempDate < card.dateOfRegistration)
                    {
                        WriteLine("Дата окончания работ должна быть больше даты постройки и регистрации данной карточки!!!");
                        flag = false;
                    }
                }
            } while (!flag);
            card.completionDate = tempDate;
            //---------------------------
            Write("Введите департамент который будет отслеживать работы: ");
            temp = ReadLine();
            card.department = temp;
            BD.AddCard(new BDCardControl.BDCart(card));
            Console.WriteLine();
        }

        public void GetCardMenu()
        {
            string temp;
            bool flag;
            Clear();
            CardObject cart = new CardObject();
            do
            {
                try
                {
                    flag = true;
                    Write("Введите название объекта: ");
                    temp = ReadLine();
                    BDCardControl.BDCart card1 = BD.GetCard(temp);
                    cart = new CardObject(card1, BD);
                    WriteLine(cart);
                }
                catch (Exception ex)
                {
                    flag = false;
                    //WriteLine(ex.Message);
                }
            } while (!flag);
            string isDownload;
            do
            {
                Write("Скачать фотографии?\n[Y] - да\n[N] - нет\nВаш выбор: ");
                isDownload = ReadLine();
                if (isDownload != "Y" && isDownload != "N")
                {
                    WriteLine("Неверный выбор!!");
                }
                else if (isDownload == "Y")
                {
                    do
                    {
                        Write("Введите путь куда скачать фотографии: ");
                        string path = ReadLine();
                        flag = Directory.Exists(path);
                        if (!flag)
                        {
                            ForegroundColor = ConsoleColor.Red;
                            WriteLine("Данная папка не существует!!");
                            ResetColor();
                        }
                        else
                        {
                            foreach (var item in cart.photos)
                            {
                                item.SaveOnDevice(path);
                            }
                        }
                    } while (!flag);

                }
            } while (isDownload != "N" && isDownload != "Y");
            do
            {
                Write("Скачать документы?\n[Y] - да\n[N] - нет\nВаш выбор: ");
                isDownload = ReadLine();
                if (isDownload != "Y" && isDownload != "N")
                {
                    WriteLine("Неверный выбор!!");
                }
                else if (isDownload == "Y")
                {
                    do
                    {
                        Write("Введите путь куда скачать документы: ");
                        string path = ReadLine();
                        flag = Directory.Exists(path);
                        if (!flag)
                        {
                            ForegroundColor = ConsoleColor.Red;
                            WriteLine("Данная папка не существует!!");
                            ResetColor();
                        }
                        else
                        {
                            foreach (var item in cart.documents)
                            {
                                item.SaveOnDevice(path);
                            }
                        }
                    } while (!flag);

                }
            } while (isDownload != "N" && isDownload != "Y");
            Console.WriteLine();
        }

        public void RemoveCardMenu()
        {
            string temp;
            bool flag;
            do
            {
                Write("Введите название объекта: ");
                temp = ReadLine();
                flag = BD.DeleteCard(temp);
            } while (!flag);
            Console.WriteLine();
        }

        public void DrawMainmenu()
        {
            int choise = 0;
            do
            {
                Clear();
                Console.Write("[0] - Выход\n[1] - Создать карточку\n[2] - Посмотреть карточку\n[3] - Удалить карточку\nВаш выбор: ");
                Int32.TryParse(ReadLine(), out choise);
                switch (choise)
                {
                    case 0:
                        Console.WriteLine();
                        break;
                    case 1:
                        AddCardMenu();
                        break;
                    case 2:
                        GetCardMenu();
                        break;
                    case 3:
                        RemoveCardMenu();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор!!");
                        break;
                }
            } while (choise != 0);
        }
    }
}

namespace ConsoleApp11
{
    internal class Program
    {
        static string connectionString = "server=localhost;user=root;password=Rotter;database=user";
        static MySqlConnection connection;
        static MySqlCommand command;


    public class ControlObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<ObjectAttribute> Attributes { get; set; }
        public ICollection<Document> Documents { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<Decision> Decisions { get; set; }
    }

    public class ObjectAttribute
    {
        public int Id { get; set; }
        public int ControlObjectId { get; set; }
        public ControlObject ControlObject { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Document
    {
        public int Id { get; set; }
        public int ControlObjectId { get; set; }
        public ControlObject ControlObject { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }

    public class Photo
    {
        public int Id { get; set; }
        public int ControlObjectId { get; set; }
        public ControlObject ControlObject { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }

    public class Decision
    {
        public int Id { get; set; }
        public int ControlObjectId { get; set; }
        public ControlObject ControlObject { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int Responsibleid { get; set; }
        public User ResponsibleUser { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public ICollection<Decision> AssignedDecisions { get; set; }
    }

        public static class PasswordHasher
        {
            private static readonly int SaltSize = 16;
            private static readonly int HashSize = 20;

            public static string HashPassword(string password)
            {
                using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, 10000))
                {
                    byte[] salt = algorithm.Salt;
                    byte[] hash = algorithm.GetBytes(HashSize);

                    return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
                }
            }

            public static bool VerifyPassword(string password, string hashedPassword)
            {
                string[] parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                {
                    throw new FormatException("The stored password is not in the correct format.");
                }

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] hash = Convert.FromBase64String(parts[1]);

                using (var algorithm = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    byte[] computedHash = algorithm.GetBytes(HashSize);
                    return ByteArraysEqual(hash, computedHash);
                }
            }

            private static bool ByteArraysEqual(byte[] a, byte[] b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (a == null || b == null || a.Length != b.Length)
                {
                    return false;
                }

                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }



        static void Main(string[] args)
        {
            
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
            } catch (Exception ex)
            {
                WriteLine("Ошибка при подключение к MySql: " + ex.Message);
                return;
            }
            TableExists();
            MainMenuAuth();
            ReadKey();
        }

        public static void MainMenuAuth()
        {
            while (true)
            {
                Clear();
                ForegroundColor = ConsoleColor.Green;
                WriteLine("\n\t\t\t╔═════════════════════════════════════╗");
                WriteLine("\t\t\t║            ГЛАВНОЕ МЕНЮ             ║");
                WriteLine("\t\t\t╚═════════════════════════════════════╝\n");
                ResetColor();

                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("\t1. Регистрация");
                WriteLine("\t2. Вход");
                WriteLine("\t0. Выйти");
                ResetColor();

                Write("\nВведите номер действия: ");
                string input = ReadLine();

                switch (input)
                {
                    case "1":
                        Register();
                        break;
                    case "2":
                        Login();
                        break;
                    case "0":
                        ForegroundColor = ConsoleColor.Yellow;
                        WriteLine("\nДо свидания!");
                        ResetColor();
                        return;
                    default:
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("\nНеверный выбор. Попробуйте еще раз.");
                        ResetColor();
                        break;
                }

                ReadLine();
            }
        }
        public static void Register()
        {
            try
            {
                WriteLine("Введите имя пользователя:");
                string username = ReadLine();

                
                string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE username = @username";
                MySqlCommand checkUsernameCommand = new MySqlCommand(checkUsernameQuery, connection);
                checkUsernameCommand.Parameters.AddWithValue("@username", username);
                int usernameCount = Convert.ToInt32(checkUsernameCommand.ExecuteScalar());

                if (usernameCount > 0)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Этот username уже занят. Пожалуйста, выберите другой.");
                    ResetColor();
                    return;
                }
                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("Введите полное имя:");
                ResetColor();
                string name = ReadLine();
                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("Введите возраст:");
                ResetColor();
                int age = int.Parse(ReadLine());
                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("Введите электронную почту:");
                ResetColor();
                string email = ReadLine();
                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("Введите пароль:");
                ResetColor();
                string password = ReadLine();
                string hashedPassword = PasswordHasher.HashPassword(password);

                string registerQuery = "INSERT INTO Users (username, name, age, email, password) VALUES (@username, @name, @age, @email, @password)";
                MySqlCommand registerCommand = new MySqlCommand(registerQuery, connection);
                registerCommand.Parameters.AddWithValue("@username", username);
                registerCommand.Parameters.AddWithValue("@name", name);
                registerCommand.Parameters.AddWithValue("@age", age);
                registerCommand.Parameters.AddWithValue("@email", email);
                registerCommand.Parameters.AddWithValue("@password", hashedPassword);

                int rowsAffected = registerCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine("Регистрация прошла успешно!");
                    ResetColor();
                    ReadKey();
                    WriteLine("Нажмите Enter чтобы продолжить...");
                    MainMenuAuth();

                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Ошибка при регистрации. Пожалуйста, попробуйте еще раз.");
                    ResetColor();
                    ReadKey();
                    WriteLine("Нажмите Enter чтобы продолжить...");
                    MainMenuAuth();
                }
            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"Ошибка при регистрации: {ex.Message}");
                ResetColor();
                ReadKey();
                WriteLine("Нажмите Enter чтобы продолжить...");
                MainMenuAuth();
            }
        }

        public static void Login()
        {
            try
            {
                WriteLine("Введите имя пользователя:");
                string username = ReadLine();

                WriteLine("Введите пароль:");
                string password = ReadLine();

                string loginQuery = "SELECT * FROM Users WHERE username = @username";
                MySqlCommand loginCommand = new MySqlCommand(loginQuery, connection);
                loginCommand.Parameters.AddWithValue("@username", username);

                MySqlDataReader reader = loginCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    string hashedPassword = reader.GetString("password");

                    if (PasswordHasher.VerifyPassword(password, hashedPassword))
                    {
                        ForegroundColor = ConsoleColor.Green;
                        WriteLine("Вход выполнен успешно!");
                        ResetColor();
                        ReadKey();
                        WriteLine("Нажмите Enter чтобы продолжить...");
                        MainMenu();


                    }
                    else
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("Неправильное имя пользователя или пароль.");
                        ResetColor();
                        ReadKey();
                        WriteLine("Нажмите Enter чтобы продолжить...");
                        MainMenuAuth();
                    }
                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Неправильное имя пользователя или пароль.");
                    ResetColor();
                    ReadKey();
                    WriteLine("Нажмите Enter чтобы продолжить...");
                    MainMenuAuth();
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"Ошибка при входе: {ex.Message}");
                ResetColor();
            }
        }


        static void MainMenu()
        {
            Clear();
            ForegroundColor = ConsoleColor.Green;

            WriteLine("\n\t\t\tГЛАВНОЕ МЕНЮ\n");
            ResetColor();

            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("\t1. Управление объектами контроля");
            WriteLine("\t2. Управление пользователями");
            WriteLine("\t0. Выход");
            ResetColor();

            Write("\nВведите номер опции: ");
            string choice = ReadLine();

            switch(choice)
            {
                case "1":
                    Clear();
                    ControlObjectMenu();
                    break;
                case "2":
                    UserMenu();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("\nНеверный выбор попробуйте снова.");
                    ResetColor();
                    ReadKey();
                    MainMenu();
                    break;
            }
        }
        static void ControlObjectMenu()
        {
            MainCardObject.BDCardControl BDCard = new BDCardControl(connectionString);
            MainCardObject.Interface interfase = new Interface(BDCard, connectionString);

            
            string choice;
            do
            {
                Clear();
                ForegroundColor = ConsoleColor.Green;
                WriteLine("\n\t\t\tУПРАВЛЕНИЕ ОБЪЕКТАМИ КОНТРОЛЯ\n");
                ResetColor();
                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("\t1. Добавить объект контроля");
                WriteLine("\t2. Посмотреть карточку объекта контроля");
                WriteLine("\t3. Удалить объект контроля");
                WriteLine("\t0. Вернуться в главное меню");
                ResetColor();
                Write("\nВведите номер опции: ");
                choice = ReadLine();

                switch (choice)
                {
                    case "1":
                        interfase.AddCardMenu();
                        break;
                    case "2":
                        interfase.GetCardMenu();
                        break;
                    case "3":
                        interfase.RemoveCardMenu();
                        break;
                    case "0":
                        MainMenu();
                        break;
                    default:
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("\nНеверный выбор попробуйте снова.");
                        ResetColor();
                        ReadKey();
                        ControlObjectMenu();
                        break;
                }
            }while (choice != "0");
        }
        static void UserMenu()
        {
            Clear();
            ForegroundColor = ConsoleColor.Green;

            WriteLine("\n\t\t\tПОЛЬЗОВАТЕЛЬ МЕНЮ\n");
            ResetColor();

            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("\t1. Добавить пользователя");
            WriteLine("\t2. Добавить пользователя  XML");
            WriteLine("\t3. Посмотреть список пользователей");
            WriteLine("\t4. Редактировать пользователя");
            WriteLine("\t5. Удалить пользователя");
            WriteLine("\t0. Вернуться в главное меню");
            ResetColor();
            Write("\nВведите номер опции: ");
            string choice = ReadLine();
            

            switch (choice)
            {
                case "1":
                    AddUser();
                    break;
                case "2":
                    AddUserXML();
                    break;
                case "3":
                    ViewUser();
                    break;
                case "4":
                    UpdateUser();
                    break;
                case "5":
                    DeleteUser();
                    break;
                case "0":
                    connection.Close();
                    MainMenu();
                    break;
                default:
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("\nНеверный выбор попробуйте снова.");
                    ResetColor();
                    ReadKey();
                    UserMenu();
                    break;
            }
        }

        static void AddUser()
        {
            


            Clear();
            ForegroundColor = ConsoleColor.Green;

            WriteLine("\n\t\t\tДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ\n");
            ResetColor();

            ForegroundColor= ConsoleColor.Cyan;
            WriteLine("Ввведите имя пользователя: ");
            ResetColor();
            string username = ReadLine();
            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("Ввведите свое ФИО: (Формат: Иванов Иван Иванович)");
            ResetColor();
            string name = ReadLine();
            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("Ввведите возраст: ");
            ResetColor();
            int age = int.Parse(ReadLine());
            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("Введите пароль: ");
            ResetColor();
            string password = ReadLine();
            ForegroundColor = ConsoleColor.Cyan;
            WriteLine("Введите почту: ");
            ResetColor();
            string email = ReadLine();
            string role = "default";

            string hashedPassword = PasswordHasher.HashPassword(password);

            string query = "INSERT INTO Users (username, role, password, email, name, age) VALUES (@username, @role, @password, @email, @name, @age)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@role", role);
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@age", age);




            try
            {
                command.ExecuteNonQuery();
                ForegroundColor = ConsoleColor.Green;
                WriteLine("Информация о пользователе была успешно добавлена в базу данных.");
                ResetColor();
                ReadKey();
                UserMenu();
              // WriteLine("Через 5 секунд вы будете возвращеные в меню...");
                //Thread.Sleep(5000);
            } catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при добавление информации о пользователе." + "\nЛог: " + ex.Message);
            }
        }
        static void ViewUser()
        {
            Clear();
            string query = "SELECT * FROM Users";
            command = new MySqlCommand(query, connection);
            try
            {
                ForegroundColor = ConsoleColor.Green;
                WriteLine("\n\t\t\tИНФОРМАЦИЯ О ПОЛЬЗОВАТЕЛЯХ\n");
                ResetColor();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string username = reader.GetString("username");
                        string role = reader.GetString("role");
                        string password = reader.GetString("password");
                        string email = reader.GetString("email");
                        string name = reader.GetString("name");
                        int age = reader.GetInt32("age");
                        int id = reader.GetInt32("id");   
                        
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine($"\t1. Id: {id}");
                        WriteLine($"\t2. Пользователь: {username}");
                        WriteLine($"\t3. Роль: {role}");
                        WriteLine($"\t4. Пароль: {password}");
                        WriteLine($"\t5. Почта: {email}");
                        WriteLine($"\t6. ФИО: {name}");
                        WriteLine($"\t7. Возраст: {age}\n\n");

                        ResetColor();
                    }
                }

                reader.Close();
            if(reader.IsClosed)
            {
                    Write("Нажмите Enter чтобы выйти в меню...");
                    ReadKey();
                    UserMenu();
            }   
              
            } catch (Exception ex )
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка во время чтение записи: " + ex.Message);
            }
            
        }
        static void TableExists()
        {
            string query = "CREATE TABLE IF NOT EXISTS Users (id INT AUTO_INCREMENT PRIMARY KEY, username VARCHAR(255),role VARCHAR(255), password VARCHAR(255),email VARCHAR(255), name VARCHAR(255), age tinyint unsigned NOT NULL)";

            command = new MySqlCommand(query,connection);


            try
            {
                command.ExecuteNonQuery();
                ForegroundColor = ConsoleColor.Green;
                WriteLine("Таблица создана или существуюет.");
                ResetColor();
            } catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка создания таблицы." + ex.Message);
            }
        }
        public static void DeleteUser()
        {
            Clear();
            string query = "SELECT id FROM Users";
            MySqlCommand command = new MySqlCommand(query, connection);
            int userId;
            try
            {
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    ForegroundColor = ConsoleColor.Cyan;
                    WriteLine("Список ID:");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("id");
                        WriteLine($"\tID: {id}");
                    }
                    ResetColor();
                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Нет пользователей в базе данных.");
                    ResetColor();
                    return;
                }

                reader.Close();
                ForegroundColor = ConsoleColor.Cyan;
                Write("Введите ID пользователя, которого вы хотите удалить: ");
                userId = int.Parse(ReadLine());

                string deleteQuery = "DELETE FROM Users WHERE id = @id";
                MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@id", userId);

                int rowsAffected = deleteCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine($"Пользователь с ID {userId} был успешно удален.");
                    ResetColor();
                    ReadKey();
                    UserMenu();
                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine($"Не удалось найти пользователя с ID {userId}.");
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
                ResetColor();
                ReadKey();
                UserMenu();
            }
        }
        public static void UpdateUser()
        {
            Clear();
            string query = "SELECT id FROM Users";
            MySqlCommand command = new MySqlCommand(query, connection);
            int userId;
            try
            {
                MySqlDataReader reader = command.ExecuteReader();

                ForegroundColor = ConsoleColor.Green;
                WriteLine("\n\t\t\tРЕДАКТИРОВАНИЯ ПОЛЬЗОВАТЕЛЯЙ\n");
                ResetColor();

                if (reader.HasRows)
                {
                    ForegroundColor = ConsoleColor.Cyan;
                    WriteLine("Список ID:");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("id");
                        WriteLine($"\tID: {id}");
                    }
                    ResetColor();
                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine("Нет пользователей в базе данных.");
                    ResetColor();
                    return;
                }

                reader.Close();
                ForegroundColor = ConsoleColor.Cyan;
                Write("\nВведите ID пользователя, которого вы хотите обновить: ");
                ResetColor();
                userId = int.Parse(ReadLine());



                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("\n\t\t\tКакой параметр вы хотите обновить?\n");
                ResetColor();

                ForegroundColor = ConsoleColor.Cyan;
                WriteLine("\t1. Имя пользователя");
                WriteLine("\t2. Полное имя");
                WriteLine("\t3. Возраст");
                WriteLine("\t4. Электронная почта");
                WriteLine("\t5. Пароль");
                WriteLine("\t6. Сохранить изменения");
                ResetColor();
                int choice = int.Parse(ReadLine());

                string updateQuery = "";
                object newValue = null;

                switch (choice)
                {
                    case 1:
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine("Введите новое имя пользователя:");
                        ResetColor();
                        string newUsername = ReadLine();
                        updateQuery = "UPDATE Users SET username = @newValue WHERE id = @id";
                        newValue = newUsername;
                        break;
                    case 2:
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine("Введите новое полное имя:");
                        ResetColor();
                        string newName = ReadLine();
                        updateQuery = "UPDATE Users SET name = @newValue WHERE id = @id";
                        newValue = newName;
                        break;
                    case 3:
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine("Введите новый возраст:");
                        ResetColor();
                        int newAge = int.Parse(ReadLine());
                        updateQuery = "UPDATE Users SET age = @newValue WHERE id = @id";
                        newValue = newAge;
                        break;
                    case 4:
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine("Введите новую электронную почту:");
                        ResetColor();
                        string newEmail = ReadLine();
                        updateQuery = "UPDATE Users SET email = @newValue WHERE id = @id";
                        newValue = newEmail;
                        break;
                    case 5:
                        ForegroundColor = ConsoleColor.Cyan;
                        WriteLine("Введите новый пароль:");
                        ResetColor();
                        string newPassword = ReadLine();
                        string hashedPassword = PasswordHasher.HashPassword(newPassword);
                        updateQuery = "UPDATE Users SET password = @newValue WHERE id = @id";
                        newValue = hashedPassword;
                        break;
                    case 6:
                        ForegroundColor = ConsoleColor.Green;
                        WriteLine("Изменения сохранены.");
                        ResetColor();
                        UserMenu();
                        return;
                    default:
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("Неверный выбор. Возвращаемся в главное меню.");
                        ResetColor();
                        return;
                }

                MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@newValue", newValue);
                updateCommand.Parameters.AddWithValue("@id", userId);

                int rowsAffected = updateCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine($"Пользователь с ID {userId} был успешно обновлен.");
                    ResetColor();
                    UpdateUser();
                }
                else
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine($"Не удалось найти пользователя с ID {userId}.");
                    ResetColor();
                    UpdateUser();
                }
            }
            catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"Ошибка при обновлении пользователя: {ex.Message}");
                ResetColor();
                UpdateUser();
            }
        }


        static void AddUserXML()
        {
            Write("Введите путь до файла: ");
            string filePath = ReadLine();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNodeList nodes = doc.SelectNodes("users/user");

                foreach (XmlNode node in nodes)
                {
                    string name = node.SelectSingleNode("name").InnerText;
                    int age = int.Parse(node.SelectSingleNode("age").InnerText);
                    string username = node.SelectSingleNode("username").InnerText;
                    string password = node.SelectSingleNode("password").InnerText;
                    string email = node.SelectSingleNode("email").InnerText;

                    string hashedPassword = PasswordHasher.HashPassword(password);

                    string query = "INSERT INTO Users (username, role, password, email, name, age) VALUES (@username, @role, @password, @email, @name, @age)";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@role", "default");
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@age", age);
                    command.ExecuteNonQuery();
                    ForegroundColor = ConsoleColor.Green;
                    WriteLine("Запись была успешно добавлена (XML)");
                    ResetColor() ;
                }
            } catch (Exception ex)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Ошибка при добовлние записи из XML файла. " + ex.Message);
            }
        }
    }
}
