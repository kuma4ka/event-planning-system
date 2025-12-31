export function initViewToggle() {
    const container = document.getElementById("itemsContainer");
    const btnGrid = document.getElementById("btnGridView");
    const btnList = document.getElementById("btnListView");

    if (!container || !btnGrid || !btnList) return;

    function setView(viewMode) {
        if (viewMode === "list") {
            container.classList.add("list-view");
            btnList.classList.add("active");
            btnGrid.classList.remove("active");
        } else {
            container.classList.remove("list-view");
            btnGrid.classList.add("active");
            btnList.classList.remove("active");
        }
        localStorage.setItem("preferredViewMode", viewMode);
    }

    const savedMode = localStorage.getItem("preferredViewMode") || "grid";
    setView(savedMode);

    btnGrid.addEventListener("click", () => setView("grid"));
    btnList.addEventListener("click", () => setView("list"));
}