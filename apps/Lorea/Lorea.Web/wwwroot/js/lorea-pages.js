// Keeps the "On this page" list in step with what the reader is looking at.
let detach = () => {};

export function watchToc() {
    detach();

    const links = Array.from(document.querySelectorAll("[data-toc]"));
    const sections = links
        .map(link => document.getElementById(link.dataset.toc))
        .filter(Boolean);

    if (sections.length === 0) {
        return;
    }

    const header = document.querySelector(".site-head");
    const offset = (header?.getBoundingClientRect().height ?? 0) + 28;
    let frame = 0;

    function mark(section) {
        for (const link of links) {
            const current = link.dataset.toc === section.id;
            link.classList.toggle("is-current", current);
            if (current) {
                link.setAttribute("aria-current", "location");
            } else {
                link.removeAttribute("aria-current");
            }
        }
    }

    function update() {
        frame = 0;

        const atEnd = window.scrollY + window.innerHeight >= document.documentElement.scrollHeight - 4;
        if (atEnd) {
            mark(sections[sections.length - 1]);
            return;
        }

        let current = sections[0];
        for (const section of sections) {
            if (section.getBoundingClientRect().top - offset <= 0) {
                current = section;
            }
        }

        mark(current);
    }

    function onScroll() {
        if (frame === 0) {
            frame = requestAnimationFrame(update);
        }
    }

    window.addEventListener("scroll", onScroll, { passive: true });
    window.addEventListener("resize", onScroll);
    update();

    detach = () => {
        window.removeEventListener("scroll", onScroll);
        window.removeEventListener("resize", onScroll);
        if (frame !== 0) {
            cancelAnimationFrame(frame);
        }
        frame = 0;
    };
}

export function stopToc() {
    detach();
    detach = () => {};
}

// On a phone the guides run sideways under the page head; start with the one being read in view.
export function revealActiveTopic() {
    const link = document.querySelector(".topic-strip a.is-current");
    const strip = link?.closest(".topic-strip");
    if (!link || !strip || strip.scrollWidth <= strip.clientWidth) {
        return;
    }

    link.scrollIntoView({ block: "nearest", inline: "center", behavior: "auto" });
}

// A code block that scrolls sideways needs to be reachable with the keyboard too.
let unwatch = null;

export function watchScrollers() {
    if (unwatch) {
        return;
    }

    const root = document.getElementById("main-content") ?? document.body;
    let frame = 0;

    function mark() {
        frame = 0;
        for (const pre of root.querySelectorAll(".os-code-box pre")) {
            if (pre.scrollWidth > pre.clientWidth + 1) {
                pre.tabIndex = 0;
            } else {
                pre.removeAttribute("tabindex");
            }
        }
    }

    function schedule() {
        if (frame === 0) {
            frame = requestAnimationFrame(mark);
        }
    }

    const observer = new MutationObserver(schedule);
    observer.observe(root, { childList: true, subtree: true });
    window.addEventListener("resize", schedule);
    mark();

    unwatch = () => {
        observer.disconnect();
        window.removeEventListener("resize", schedule);
        if (frame !== 0) {
            cancelAnimationFrame(frame);
        }
        frame = 0;
    };
}
