// Inject title + right-aligned role badge
document.addEventListener("DOMContentLoaded", function () {
    var brand = document.querySelector(".navbar-brand");
    if (brand) {
        brand.style.cssText = "display:inline-flex;align-items:center;gap:10px;padding:4px 0;";
        Array.from(brand.childNodes).forEach(function(n) {
            var keep = n.nodeType === 1 && (n.tagName === "IMG" || n.tagName === "svg" || n.tagName === "SVG");
            if (!keep) { n.parentNode && n.parentNode.removeChild(n); }
        });
        var icon = brand.querySelector("img, svg");
        if (icon) { icon.style.cssText = "height:28px;width:28px;flex-shrink:0;"; }
        var title = document.createElement("span");
        title.textContent = "MCP Server";
        title.style.cssText = "font-size:1.05rem;font-weight:700;color:#fff;letter-spacing:.06em;text-transform:uppercase;white-space:nowrap;";
        brand.appendChild(title);
    }
    var navContainer = document.querySelector(".navbar .container");
    if (navContainer) {
        navContainer.style.position = "relative";
        var badge = document.createElement("span");
        badge.textContent = "Automator";
        badge.style.cssText = [
            "position:absolute",
            "top:50%",
            "right:71px",
            "transform:translateY(-50%)",
            "font-size:.85rem",
            "font-weight:700",
            "letter-spacing:.06em",
            "text-transform:uppercase",
            "padding:.25rem .7rem",
            "border-radius:4px",
            "border-left:3px solid #22d3a0",
            "background:rgba(34,211,160,.1)",
            "color:#22d3a0",
            "white-space:nowrap",
            "line-height:1.6",
            "z-index:10"
        ].join(";");
        navContainer.appendChild(badge);
    }
});

// Hide inherited members section
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("h1, h2, h3, h4, h5, h6").forEach(function (heading) {
        if (!heading.textContent.trim().replace(/\s+/g, ' ').includes("Inherited Members")) return;
        heading.style.display = "none";
        var level = parseInt(heading.tagName[1]);
        var sibling = heading.nextElementSibling;
        while (sibling) {
            var sibLevel = sibling.tagName.match(/^H([1-6])$/);
            if (sibLevel && parseInt(sibLevel[1]) <= level) break;
            var next = sibling.nextElementSibling;
            sibling.style.display = "none";
            sibling = next;
        }
    });
});

const sw = document.getElementById("switch-style"), b = document.body;
if (sw && b) {
    sw.checked = window.localStorage && localStorage.getItem("theme") === "theme-dark" || !window.localStorage;
    b.classList.toggle("theme-dark", sw.checked);
    b.classList.toggle("theme-light", !sw.checked);
    sw.addEventListener("change", function () {
        b.classList.toggle("theme-dark", this.checked);
        b.classList.toggle("theme-light", !this.checked);
        if (window.localStorage) {
            localStorage.setItem("theme", this.checked ? "theme-dark" : "theme-light");
        }
    });
}
