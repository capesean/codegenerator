/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("relationships", relationships);
    relationships.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "relationshipResource", "entityResource"];
    function relationships($scope, $state, $q, $timeout, notifications, appSettings, relationshipResource, entityResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = {};
        vm.runSearch = runSearch;
        vm.goToRelationship = function (projectId, entityId, relationshipId) { return $state.go("app.relationship", { projectId: projectId, entityId: entityId, relationshipId: relationshipId }); };
        vm.sortOptions = { stop: sortItems, handle: "i.sortable-handle", axis: "y" };
        vm.moment = moment;
        initPage();
        function initPage() {
            var promises = [];
            promises.push(entityResource.query({
                pageSize: 0
            }, function (data, headers) {
                vm.entities = data;
            }, function (err) {
                notifications.error("Failed to load the entities.", "Error", err);
                $state.go("app.home");
            }).$promise);
            $q.all(promises).finally(function () { return runSearch(0); });
        }
        function runSearch(pageIndex) {
            vm.loading = true;
            var promises = [];
            promises.push(relationshipResource.query({
                parentEntityId: vm.search.parentEntityId,
                childEntityId: vm.search.childEntityId,
                pageSize: 0
            }, function (data, headers) {
                vm.relationships = data;
            }, function (err) {
                notifications.error("Failed to load the relationships.", "Error", err);
                $state.go("app.home");
            }).$promise);
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        ;
        function sortItems() {
            vm.loading = true;
            var ids = [];
            angular.forEach(vm.relationships, function (item, index) {
                ids.push(item.relationshipId);
            });
            relationshipResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Relationship Ordering");
            }, function (err) {
                notifications.error("Failed to sort the relationships. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
    }
    ;
}());
//# sourceMappingURL=relationships.js.map