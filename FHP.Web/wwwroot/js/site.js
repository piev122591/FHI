(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var toggle = document.getElementById("sidebarToggle");
        var sidebar = document.getElementById("sidebar");
        var overlay = document.getElementById("sidebarOverlay");

        if (!toggle || !sidebar || !overlay) {
            return;
        }

        function openSidebar() {
            sidebar.classList.remove("-translate-x-full");
            overlay.classList.remove("hidden");
        }

        function closeSidebar() {
            sidebar.classList.add("-translate-x-full");
            overlay.classList.add("hidden");
        }

        toggle.addEventListener("click", function () {
            if (sidebar.classList.contains("-translate-x-full")) {
                openSidebar();
            } else {
                closeSidebar();
            }
        });

        overlay.addEventListener("click", closeSidebar);

        function setupSubmenuToggle(toggleId, submenuId, chevronId) {
            var toggleEl = document.getElementById(toggleId);
            var submenuEl = document.getElementById(submenuId);
            var chevronEl = document.getElementById(chevronId);

            if (!toggleEl || !submenuEl || !chevronEl) {
                return;
            }

            toggleEl.addEventListener("click", function () {
                var expanded = toggleEl.getAttribute("aria-expanded") === "true";
                var next = !expanded;
                toggleEl.setAttribute("aria-expanded", String(next));
                submenuEl.classList.toggle("hidden", !next);
                submenuEl.classList.toggle("flex", next);
                chevronEl.classList.toggle("rotate-180", next);
            });
        }

        setupSubmenuToggle("loginUserToggle", "loginUserSubmenu", "loginUserChevron");
        setupSubmenuToggle("masterSetupToggle", "masterSetupSubmenu", "masterSetupChevron");
        setupSubmenuToggle("memberManagementToggle", "memberManagementSubmenu", "memberManagementChevron");
        setupSubmenuToggle("redemptionToggle", "redemptionSubmenu", "redemptionChevron");
        setupSubmenuToggle("salesManagementToggle", "salesManagementSubmenu", "salesManagementChevron");
        setupSubmenuToggle("walletTopupToggle", "walletTopupSubmenu", "walletTopupChevron");
        setupSubmenuToggle("walletToggle", "walletSubmenu", "walletChevron");
        setupSubmenuToggle("warehouseManagerToggle", "warehouseManagerSubmenu", "warehouseManagerChevron");
        setupSubmenuToggle("bonusOperationToggle", "bonusOperationSubmenu", "bonusOperationChevron");
        setupSubmenuToggle("customerSupportToggle", "customerSupportSubmenu", "customerSupportChevron");

        document.querySelectorAll(".delete-form").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                var username = form.dataset.username || "";
                if (!window.confirm("Delete user \"" + username + "\"? This cannot be undone.")) {
                    e.preventDefault();
                }
            });
        });
    });
})();
