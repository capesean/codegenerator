/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("entities", entities);
    entities.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "entityResource"];
    function entities($scope, $state, $q, $timeout, notifications, appSettings, entityResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.searchObjects = {};
        vm.runSearch = runSearch;
        vm.goToEntity = function (projectId, entityId) { return $state.go("app.entity", { projectId: projectId, entityId: entityId }); };
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
            var promise = entityResource.query(vm.search, function (data, headers) {
                vm.entities = data;
                vm.headers = JSON.parse(headers("X-Pagination"));
            }, function (err) {
                notifications.error("Failed to load the entities.", "Error", err);
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
//# sourceMappingURL=entities.js.map