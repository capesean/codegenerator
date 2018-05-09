/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("lookupOptions", lookupOptions);
    lookupOptions.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "lookupOptionResource"];
    function lookupOptions($scope, $state, $q, $timeout, notifications, appSettings, lookupOptionResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.searchObjects = {};
        vm.runSearch = runSearch;
        vm.goToLookupOption = function (projectId, lookupId, lookupOptionId) { return $state.go("app.lookupOption", { projectId: projectId, lookupId: lookupId, lookupOptionId: lookupOptionId }); };
        vm.moment = moment;
        initPage();
        function initPage() {
            var promises = [];
            promises.push(runSearch(0, true));
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        function runSearch(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            vm.search.includeEntities = true;
            vm.search.pageIndex = pageIndex;
            var promise = lookupOptionResource.query(vm.search, function (data, headers) {
                vm.lookupOptions = data;
                vm.headers = JSON.parse(headers("X-Pagination"));
            }, function (err) {
                notifications.error("Failed to load the lookup options.", "Error", err);
                $state.go("app.home");
            }).$promise;
            if (!dontSetLoading)
                promise.then(function () { return vm.loading = false; });
            return promise;
        }
        ;
    }
    ;
}());
//# sourceMappingURL=lookupoptions.js.map