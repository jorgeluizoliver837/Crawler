using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using MySqlConnector;

namespace ConsoleApplication1
{
  internal class Program
  {
    
    public static List<string> listaurls=  new List<string>();

    public static string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    
    
    public static void Main(string[] args)
    {
      
       ScrapAllPages();
      

    }

 
         
      
      
      
           
      
      
    private static async Task ScrapAllPages()
    {

      var url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/";
      var httpclient = new HttpClient();
      var html = await httpclient.GetStringAsync(url);


      var htmlDocument = new HtmlDocument();
      htmlDocument.LoadHtml(html);
      
      //--------------------------------------------------------------------------------------
      
      List<string> listapags=  new List<string>();
      
      string ultimapag;
      
      var links = htmlDocument.DocumentNode.Descendants("a")
        .Where(node => node.GetAttributeValue("class", "")
          .Equals("page-link")).ToList();
      foreach (var a in links)
      {
        listapags.Add(a.GetAttributeValue("href","").ToString());
        
      }
      
      
      

      listaurls.Add("https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/");
      
      if (listapags.Count > 1)
      {
        ultimapag=  listapags.Last().Replace("/proxy/list/order/updated/order_dir/desc/page/", "");
        
        int lastpage = int.Parse(ultimapag);

        for (int i = 2; i < 3; i++)
        {
          listaurls.Add("https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/" + i.ToString());
        }
      }
     
      //------------------------------------------------------------------------------------------   

      Scrapdata();

      
    }
      
      private static async Task Scrapdata()
          {
            
            try
            {
            string horainicio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            DataTable table = new DataTable();

            int pag = 1;

            foreach (var linha in listaurls)
            {
              var url = linha;
              
              var httpclient = new HttpClient();
              
              var html = await httpclient.GetStringAsync(url);


              var htmlDocument = new HtmlDocument();
              
              htmlDocument.LoadHtml(html);

              var patharq = caminho + "/ArquivosCrowler/pag" + pag.ToString() + ".html";
              
              if (!File.Exists(@patharq))
              {
                
                File.WriteAllText(@patharq,html);
              
              }
              else
              {
                File.Delete(@patharq);
                File.WriteAllText(@patharq,html);
              }
              
              var headers = htmlDocument.DocumentNode.SelectNodes("//tr/th");
              
             
              
              if (table.Rows.Count == 0)
              {
                foreach (HtmlNode header in headers)

                  table.Columns.Add(header.InnerText);
              }

              foreach (var ln in htmlDocument.DocumentNode.SelectNodes("//tr[td]"))
                
                table.Rows.Add(ln.SelectNodes("td").Select(td => td.InnerText).ToArray());

              pag++;

            }
              
            table.Columns.Remove("Updated");
            table.Columns.Remove("Speed");
            table.Columns.Remove("Online");
            table.Columns.Remove("Anonymity");
            
            string horafim = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var Jsontable = DataTable_JSON_StringBuilder(table);
            
            var jsonfile = caminho + "/ArquivosCrowler/run.json";
            
            if (File.Exists(@jsonfile))
            {
                
              File.Delete(@jsonfile);
              
            }
            
            System.IO.File.WriteAllText(@jsonfile, Jsontable);
            
            
            string connString = "Server=localhost;Database=webcrawler;Uid=root;Pwd=gi231079;AllowUserVariables=True;"; // Credenciais BANCO DE DADOS
            
            MySqlConnection mcon = new MySqlConnection(connString);
            
            mcon.Open();
            
            string cmdText= "INSERT INTO TABLE_RUNS(datainicio, datafim,qtdpaginas,arqjson) VALUES (?datainicio, ?datafim,?paginas, ?jsondata)";
            
            MySqlCommand cmd = new MySqlCommand(cmdText, mcon);
            
            cmd.Parameters.AddWithValue("@datainicio", horainicio);
           
            cmd.Parameters.AddWithValue("@datafim", horafim);
            
            cmd.Parameters.AddWithValue("@paginas", pag-1);
        
            cmd.Parameters.AddWithValue("@jsondata", Jsontable.Replace("\n","\\n"));
            
            cmd.ExecuteNonQuery();
            
            Console.WriteLine("Fineshed");
            }
            
          catch (Exception e)
          {
           
          }
          }
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
      
          public static string DataTable_JSON_StringBuilder(DataTable tabela)
          {
            var JSONString = new StringBuilder();
            if (tabela.Rows.Count > 0)
            {
              JSONString.Append("[");
              for (int i = 0; i < tabela.Rows.Count; i++)
              {
                JSONString.Append("{");
                for (int j = 0; j < tabela.Columns.Count; j++)
                {
                  if (j < tabela.Columns.Count - 1)
                  {
                    JSONString.Append("\"" + tabela.Columns[j].ColumnName.ToString() + "\":" + "\"" + tabela.Rows[i][j].ToString() + "\",");
                  }
                  else if (j == tabela.Columns.Count - 1)
                  {
                    JSONString.Append("\"" + tabela.Columns[j].ColumnName.ToString() + "\":" + "\"" + tabela.Rows[i][j].ToString() + "\"");
                  }
                }
                if (i == tabela.Rows.Count - 1)
                {
                  JSONString.Append("}");
                }
                else
                {
                  JSONString.Append("},");
                }
              }
              JSONString.Append("]");
            }
            return JSONString.ToString();
          }
      
      

  }
}