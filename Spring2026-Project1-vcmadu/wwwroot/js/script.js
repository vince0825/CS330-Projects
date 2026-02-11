// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Put the current year in the footer
document.getElementById('year').textContent = new Date().getFullYear();

// Optional: color the rating number
(function () {
  const span = document.querySelector('#Rating span');
  if (!span) return;
  const n = parseFloat(span.textContent);
  if (isNaN(n)) return;
  const el = document.getElementById('Rating');
  if (n >= 7.5) el.style.color = '#7CFC00';     // green
  else if (n >= 6) el.style.color = '#FFD700';  // yellow
  else el.style.color = '#FF6B6B';              // red
})();