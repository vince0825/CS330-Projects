// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const API_KEY = "AIzaSyDH0N9smjzeFISq1lKwTtsGRb33Gu_KG94";
const CX      = "20866f71a5e554d6a";


// Backgrounds to cycle
const BG_IMAGES = [
  "https://images.unsplash.com/photo-1519681393784-d120267933ba?q=80&w=2400&auto=format&fit=crop",
  "https://images.unsplash.com/photo-1680699641482-419ff446a7d5?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
  "https://images.unsplash.com/photo-1469474968028-56623f02e42e?q=80&w=2400&auto=format&fit=crop"
];

let bgIndex = 0;

/* Apply a background image by index.
   The rest of the background properties (position/size/fixed)
   are set in CSS, so we only set background-image here. */
function applyBackground(index) {
  document.body.style.backgroundImage = `url('${BG_IMAGES[index]}')`;
}

/* Escape HTML to avoid layout issues when rendering snippets. */
function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (m) => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]
  ));
}

/* Render Google results (title → link → snippet) and unhide results box. */
function renderResults(q, items) {
  let html = `<div class="results">`;
  if (!items || !items.length) {
    html += `<p class="snippet">No results for <strong>${escapeHtml(q)}</strong>.</p>`;
  } else {
    for (const it of items) {
      const title = it.title || "(no title)";
      const link = it.link || "#";
      const snippet = it.snippet || "";
      const displayLink = it.displayLink || link;

      html += `
        <div class="result">
          <a class="title" href="${link}" target="_blank" rel="noopener noreferrer">${escapeHtml(title)}</a>
          <div class="link">${escapeHtml(displayLink)}</div>
          <div class="snippet">${escapeHtml(snippet)}</div>
        </div>`;
    }
  }
  html += `</div>`;

  $("#searchResults").html(html)
                     .css("visibility", "visible"); // unhide after use
}

/* Build the Google CSE request URL from a query string. */
function buildSearchUrl(q) {
  const url = new URL("https://www.googleapis.com/customsearch/v1");
  url.searchParams.set("key", API_KEY);
  url.searchParams.set("cx", CX);
  url.searchParams.set("q", q);
  return url.toString();
}

/* DOM ready */
$(function () {
  // Ensure background shows on first load (matches your CSS initial image idea)
  applyBackground(bgIndex);

  // Enter key in the input triggers search
  $("#query").on("keydown", (e) => {
    if (e.key === "Enter") $("#searchBtn").trigger("click");
  });

  // SEARCH button
  $("#searchBtn").on("click", async () => {
    const q = $("#query").val().trim();
    if (!q) return;

    try {
      const resp = await fetch(buildSearchUrl(q));
      if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
      const data = await resp.json();
      renderResults(q, data.items || []);
    } catch (err) {
      console.error("Search failed:", err);
      renderResults(q, []); // show "no results" state
      alert("Search failed. Check API key/engine ID and network.");
    }
  });

  // I'M FEELING LUCKY (bonus)
  $("#luckyBtn").on("click", async () => {
    const q = $("#query").val().trim();
    if (!q) return;

    try {
      const resp = await fetch(buildSearchUrl(q));
      if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
      const data = await resp.json();
      const first = (data.items && data.items[0]) ? data.items[0].link : null;
      if (first) {
        window.location.href = first;
      } else {
        alert("No results found.");
      }
    } catch (e) {
      console.error("Lucky failed:", e);
      alert("Lucky failed. Check API key/engine ID.");
    }
  });

  // TIME dialog (HH:MM) — unhide then open as jQuery UI dialog
  $("#timeBtn").on("click", () => {
    const now = new Date();
    const hh = String(now.getHours()).padStart(2, "0");
    const mm = String(now.getMinutes()).padStart(2, "0");
    $("#time").text(`${hh}:${mm}`)
              .css("visibility", "visible"); // unhide after use

    if ($("#time").hasClass("ui-dialog-content")) {
      $("#time").dialog("open");
    } else {
      $("#time").dialog({ modal: true, width: 320 });
    }
  });

  // CYCLE BACKGROUND when clicking/pressing Enter/Space on the site name
  $("#brand").on("click keydown", (e) => {
    if (e.type === "click" || e.key === "Enter" || e.key === " ") {
      bgIndex = (bgIndex + 1) % BG_IMAGES.length;
      applyBackground(bgIndex);
    }
  });
});