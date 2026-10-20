// Brings the picked package into view when the wiring diagram is wider than its frame.
export function reveal(id) {
    const node = document.getElementById(id);
    const canvas = node?.closest(".map-canvas");
    if (!node || !canvas || canvas.scrollWidth <= canvas.clientWidth) {
        return;
    }

    const box = node.getBoundingClientRect();
    const view = canvas.getBoundingClientRect();
    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    canvas.scrollBy({
        left: box.left + box.width / 2 - (view.left + view.width / 2),
        behavior: reduce ? "auto" : "smooth"
    });
}
