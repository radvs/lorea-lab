let detach = () => {};

export function watchHomeViews() {
    detach();

    const explorer = document.querySelector(".home-explorer");
    if (!explorer) {
        return;
    }

    const viewLinks = Array.from(explorer.querySelectorAll(".explorer-mode"));

    function setView(view) {
        if (view !== "map" && view !== "packages") {
            return;
        }

        explorer.dataset.view = view;
        for (const link of viewLinks) {
            if (link.hash === `#${view}`) {
                link.setAttribute("aria-current", "location");
            } else {
                link.removeAttribute("aria-current");
            }
        }
    }

    function onClick(event) {
        if (!(event.target instanceof Element)) {
            return;
        }

        const link = event.target.closest("a[href]");
        if (!link) {
            return;
        }

        const target = new URL(link.href, location.href);
        if (target.origin !== location.origin || target.pathname !== location.pathname) {
            return;
        }

        const view = target.hash.slice(1);
        if (view !== "map" && view !== "packages") {
            return;
        }

        event.preventDefault();
        if (location.hash !== target.hash) {
            history.pushState(null, "", target.hash);
        }
        setView(view);
    }

    function onHistoryChange() {
        setView(location.hash.slice(1) || "map");
    }

    document.addEventListener("click", onClick, true);
    window.addEventListener("popstate", onHistoryChange);
    window.addEventListener("hashchange", onHistoryChange);
    detach = () => {
        document.removeEventListener("click", onClick, true);
        window.removeEventListener("popstate", onHistoryChange);
        window.removeEventListener("hashchange", onHistoryChange);
    };
    onHistoryChange();
}

export function stopHomeViews() {
    detach();
    detach = () => {};
}
