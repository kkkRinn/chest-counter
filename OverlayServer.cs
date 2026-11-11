using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChestCounter.Models;

namespace ChestCounter.Services;

/// <summary>Serves an OBS browser-source page at http://localhost:PORT/ (JSON at /api/stats).</summary>
public class OverlayServer : IDisposable
{
    private readonly Func<StatsSnapshot> _stats;
    private HttpListener _listener;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public int Port { get; private set; }
    public bool Running => _listener != null && _listener.IsListening;

    public OverlayServer(Func<StatsSnapshot> stats) => _stats = stats;

    public void Start(int port)
    {
        if (Running && port == Port) return;
        Stop();
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();
            Port = port;
            var l = _listener;
            Task.Run(() => Loop(l));
            Log.Write("Overlay server on port " + port);
        }
        catch (Exception ex)
        {
            Log.Write("Overlay server failed: " + ex.Message);
            _listener = null;
        }
    }

    private async Task Loop(HttpListener l)
    {
        while (l.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await l.GetContextAsync(); }
            catch { break; }
            _ = Task.Run(() => Handle(ctx));
        }
    }

    private void Handle(HttpListenerContext ctx)
    {
        try
        {
            bool api = ctx.Request.Url.AbsolutePath == "/api/stats";
            string body = api ? JsonSerializer.Serialize(_stats(), Json) : Page;
            byte[] bytes = Encoding.UTF8.GetBytes(body);

            ctx.Response.ContentType = (api ? "application/json" : "text/html") + "; charset=utf-8";
            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctx.Response.AddHeader("Cache-Control", "no-store");
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }
        catch
        {
            try { ctx.Response.Abort(); } catch { }
        }
    }

    public void Stop()
    {
        try { _listener?.Close(); } catch { }
        _listener = null;
    }

    public void Dispose() => Stop();

    private const string Page = """
<!doctype html>
<html><head><meta charset="utf-8"><title>Chest Counter</title>
<style>
  html,body{margin:0;background:transparent;font-family:"Segoe UI",Arial,sans-serif;color:#fff}
  .box{display:inline-flex;gap:32px;padding:16px 24px;border-radius:16px;
       background:rgba(10,14,26,.85);border:1px solid rgba(255,255,255,.08)}
  .k{font-size:13px;color:#8693b0}
  .v{font-size:46px;font-weight:700;line-height:1.05}
  .c{color:#2ec4d6}.o{color:#f5a524}
</style></head>
<body>
<div class="box">
  <div><div class="k">Total chests</div><div class="v c" id="total">0</div></div>
  <div><div class="k">Last game</div><div class="v o" id="last">0</div></div>
  <div><div class="k">Today</div><div class="v" id="today">0</div></div>
  <div><div class="k">Avg per game</div><div class="v" id="avg">0</div></div>
</div>
<script>
async function tick(){
  try{
    const s = await (await fetch('/api/stats')).json();
    document.getElementById('total').textContent = s.total.toLocaleString();
    document.getElementById('last').textContent = s.lastGame;
    document.getElementById('today').textContent = s.today;
    document.getElementById('avg').textContent = s.avgPerGame;
  }catch(e){}
}
tick(); setInterval(tick, 2000);
</script>
</body></html>
""";
}
