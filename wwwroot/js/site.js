// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function toggleDateRange(selectedValue) {
    const isCustom = selectedValue === "Custom";
    document.getElementById("customDateRange").classList.toggle("d-none", !isCustom);
    document.getElementById("customDateRangeTo").classList.toggle("d-none", !isCustom);
}
