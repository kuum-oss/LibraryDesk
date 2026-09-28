using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryDesk.Core
{
// клас
public class vidacha
{
public string id;
public string cl;
public List<string[]> ln = new List<string[]>();
 public int st = 0; // 0-новий,1-оплач,2-відпр,3-скасов
public DateTime dt;
// public string prim; // примітка, поки не треба
// конструктор
 public vidacha(string a, string b)
{
id = a; // ставимо id
 cl = b; // ставимо cl
dt = DateTime.Now; // ставимо дату
}
// метод додавання
public void Add(string s, int k, decimal c)
{
string[] t = new string[3];
 t[0] = s; t[1] = k.ToString(); t[2] = c.ToString(); ln.Add(t); // додаємо t у ln
}
// ProcessData
public decimal ProcessData(bool f)
{
decimal sum1 = 0; int kolvo = 0;
for (int i = 0; i < ln.Count; i++)
{
int k = int.Parse(ln[i][1]);
 decimal c = decimal.Parse(ln[i][2]);
sum1 = sum1 + k * c; // додаємо до суми
kolvo = kolvo + 1; // збільшуємо kolvo на одиницю
}
// if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
if (f == true && sum1 > 1000) { sum1 = sum1 * 0.9m; } else if (sum1 > 5000) { sum1 = sum1 * 0.85m; } else { sum1 = sum1; }
if (kolvo > 10) sum1 = sum1 - 100;
 if (sum1 < 0) sum1 = 0;
sum1 = sum1 + sum1 * 0.2m;
return Math.Round(sum1, 2); // повертаємо sum1
}
// міняємо статус
public bool Chg(int n)
{
if (st == 0 && n == 1) { st = 1; return true; }
if (st == 1 && n == 2) { st = 2; return true; }
if (st == 0 && n == 3) { st = 3; return true; }
return false; // не можна
}
// перевірка
public bool Ck()
{
if (id != null && id != "" && cl != null && cl.Length > 2 && ln.Count > 0 && ln.Count < 100 && st >= 0 && st <= 3) return true;
else
return false;
}
// звіт
public string Rep()
{
string s = "";
for (int i = 0; i < ln.Count; i++)
{
s = s + "Товар: " + ln[i][0] + "; кількість: " + ln[i][1] + "; ціна: " + ln[i][2] + "; сума: " + (int.Parse(ln[i][1]) * decimal.Parse(ln[i][2])) + "\n";
}
s = s + "Разом: " + ProcessData(false) + "\n";
return s; // повертаємо s
}
// пошук
public static vidacha F(List<vidacha> L, string x)
{
for (int i = 0; i < L.Count; i++)
{
if (L[i].id == x) { return L[i]; }
}
return null;
}
}
}
