window.lorea = (() => {
    const storageKey = "lorea.culture";

    function getPreferredCulture() {
        try {
            return localStorage.getItem(storageKey);
        } catch (error) {
            console.error("Could not read the saved Lorea language preference.", error);
            return null;
        }
    }

    function setPreferredCulture(culture) {
        try {
            localStorage.setItem(storageKey, culture);
        } catch (error) {
            console.error("Could not save the Lorea language preference.", error);
        }
    }

    function setDocumentCulture(language, direction) {
        document.documentElement.lang = language;
        document.documentElement.dir = direction;
    }

    // The dependency map is readable at its own size on a phone and fitted on a desk.
    function wideScreen() {
        return window.matchMedia("(min-width: 64rem)").matches;
    }

    function preloadFont(file) {
        const link = document.createElement("link");
        link.rel = "preload";
        link.as = "font";
        link.type = "font/woff2";
        link.crossOrigin = "";
        link.href = new URL(`fonts/woff2/${file}`, document.baseURI).href;
        document.head.appendChild(link);
    }

    const savedCulture = getPreferredCulture();
    if (savedCulture) {
        setDocumentCulture(savedCulture.split("-")[0], savedCulture === "fa-IR" ? "rtl" : "ltr");
    }

    if (savedCulture === "fa-IR") {
        preloadFont("irs-fn-reg.woff2");
        preloadFont("irs-fn-bold.woff2");
    }

    return { getPreferredCulture, setPreferredCulture, setDocumentCulture, wideScreen };
})();
