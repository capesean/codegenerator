/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("codeReplacements", codeReplacements);
    codeReplacements.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "codeReplacementResource"];
    function codeReplacements($scope, $state, $q, $timeout, notifications, appSettings, codeReplacementResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.runSearch = runSearch;
        vm.goToCodeReplacement = function (projectId, entityId, codeReplacementId) { return $state.go("app.codeReplacement", { projectId: projectId, entityId: entityId, codeReplacementId: codeReplacementId }); };
        vm.sortOptions = { stop: sortItems, handle: "i.sortable-handle", axis: "y" };
        vm.moment = moment;
        initPage();
        function initPage() {
            var promises = [];
            $q.all(promises).finally(function () { return runSearch(0); });
        }
        function runSearch(pageIndex) {
            vm.loading = true;
            var promises = [];
            promises.push(codeReplacementResource.query({
                findCode: vm.search.findCode,
                replacementCode: vm.search.replacementCode,
                q: vm.search.q,
                pageSize: 0
            }, function (data, headers) {
                vm.codeReplacements = data;
            }, function (err) {
                notifications.error("Failed to load the code replacements.", "Error", err);
                $state.go("app.home");
            }).$promise);
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        ;
        function sortItems() {
            vm.loading = true;
            var ids = [];
            angular.forEach(vm.codeReplacements, function (item, index) {
                ids.push(item.codeReplacementId);
            });
            codeReplacementResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Code Replacement Ordering");
            }, function (err) {
                notifications.error("Failed to sort the code replacements. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
    }
    ;
}());
//# sourceMappingURL=codereplacements.js.map