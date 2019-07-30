using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CinemaSearch.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Data.SQLite;
using System.Collections.Generic;

namespace CinemaSearch.Controllers
{
    public class HomeController : Controller
    {
        public List<string[]> display = new List<string[]> { };
        public string EditText;

        public string Title_Movie;
        public string URL_Poster;
        public string Overview_Movie;
        public string Vote_Movie;

        public IActionResult Index()
        {
            return View();
        }

        /*
         *Search function
         */
        [HttpPost]
        public IActionResult Search()
        {

            try
            {
                SQLiteConnection connection = new SQLiteConnection("Data Source=mywatchlist.db;Version=3;New=False;Compress=True;");
                connection.Open();
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE watchlist (Title varchar, Day varchar)";
                command.ExecuteNonQuery();
            }catch(Exception e)
            {
                //
            }

            string resp = "";
            EditText = Request.Form["moviename"];
            EditText = EditText.Replace(' ', '+');
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.themoviedb.org/3/search/movie?api_key=a3bdaae66f8cf705750820e17c0e9471&query=" + EditText);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                resp = reader.ReadToEnd();
                dynamic data = JObject.Parse(resp);

                /*
                * JSON
                */

                try
                {
                    Title_Movie = data.results[0].title;
                    URL_Poster = Url.Content("https://image.tmdb.org/t/p/w300" +data.results[0].poster_path);
                    Vote_Movie = data.results[0].vote_average;
                    Overview_Movie = data.results[0].overview;

                }catch(Exception e)
                {
                    //Handle Error
                }

            }
            ViewData["Title"] = Title_Movie;
            ViewData["URL"] = URL_Poster;
            ViewData["Overview"] = Overview_Movie;
            ViewData["Vote"] = Vote_Movie;

            return View();
        }

        [HttpPost]
        public ActionResult Create(IFormCollection formCollection)
        {
            /*
             * Setting varibales from layout forms
             */
            string moviename = Request.Form["moviename"];
            moviename = moviename.Replace(' ', '+');
            string day = Request.Form["wday"];

            SQLiteConnection connection = new SQLiteConnection("Data Source=mywatchlist.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            /*
             * Executing insert query
             */
            command.CommandText = "INSERT INTO watchlist (Title, Day) VALUES (" + "'" + moviename + "'," + "'" + day + "'" + ")";
            command.ExecuteNonQuery();

            return View();

        }

        [HttpPost]
        public ActionResult Delete(IFormCollection formCollection)
        {
            string moviename = Request.Form["moviename"];
            moviename = moviename.Replace(' ', '+');

            SQLiteConnection connection = new SQLiteConnection("Data Source=mywatchlist.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

           
            command.CommandText = "DELETE FROM watchlist WHERE Title = " + "'" + moviename + "'";
            command.ExecuteNonQuery();

            return View();

        }

        public IActionResult Watchlist()
        {
            SQLiteConnection connection = new SQLiteConnection("Data Source=mywatchlist.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            
            command.CommandText = "SELECT * FROM watchlist";

            SQLiteDataReader sqliteReader = command.ExecuteReader();

            while (sqliteReader.Read())
            {

               
                String[] movieDetails = new string[2];

         
                movieDetails[0] = sqliteReader.GetString(0);
                movieDetails[1] = sqliteReader.GetString(1);

                display.Add(movieDetails);
            }

            ViewData["Display"] = display;

            return View();
        }

        [HttpPost]
        public IActionResult Update()
        {
            string moviename = Request.Form["moviename"];
            moviename = moviename.Replace(' ', '+');
            string wday = Request.Form["wday"];

            SQLiteConnection connection = new SQLiteConnection("Data Source=mywatchlist.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "UPDATE watchlist SET Day=" + "'"+wday+"'" + " WHERE Title = " + "'" + moviename + "'";
            command.ExecuteNonQuery();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
