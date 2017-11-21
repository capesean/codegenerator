/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("project", project);

    project.$inject = ["$scope", "$state", "$stateParams", "projectResource", "notifications", "appSettings", "$q", "errorService", "lookupResource", "entityResource", "utilitiesResource"];
    function project($scope, $state, $stateParams, projectResource, notifications, appSettings, $q, errorService, lookupResource, entityResource, utilitiesResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.projectId === vm.appSettings.newGuid;
        vm.goToLookup = (projectId, lookupId) => $state.go("app.lookup", { projectId: projectId, lookupId: lookupId });
        vm.loadLookups = loadLookups;
        vm.goToEntity = (projectId, entityId) => $state.go("app.entity", { projectId: projectId, entityId: entityId });
        vm.loadEntities = loadEntities;
        vm.multideploy = {};
        vm.setCheckBoxes = setCheckBoxes;
        vm.setCheckBoxesByRow = setCheckBoxesByRow;
        vm.selectAll = selectAll;
        vm.runMultiDeploy = runMultiDeploy;

        initPage();

        function initPage() {

            var promises = [];

            $q.all(promises)
                .then(() => {

                    if (vm.isNew) {

                        vm.project = new projectResource();
                        vm.project.projectId = appSettings.newGuid;
                        vm.loading = false;

                    } else {

                        promises = [];

                        promises.push(
                            projectResource.get(
                                {
                                    projectId: $stateParams.projectId
                                },
                                data => {
                                    vm.project = data;
                                },
                                err => {

                                    if (err.status === 404) {
                                        notifications.error("The requested project does not exist.", "Error");
                                    } else {
                                        notifications.error("Failed to load the project.", "Error", err);
                                    }

                                    $state.go("app.projects");

                                })
                                .$promise);

                        promises.push(loadLookups(0));
                        promises.push(loadEntities(0));

                        $q.all(promises).finally(() => vm.loading = false);
                    }
                });
        }

        function save() {

            if ($scope.mainForm.$invalid) {

                notifications.error("The form has not been completed correctly.", "Error");

            } else {

                vm.loading = true;

                vm.project.$save(
                    data => {

                        vm.project = data;
                        notifications.success("The project has been saved.", "Saved");
                        if (vm.isNew)
                            $state.go("app.project", {
                                projectId: vm.project.projectId
                            });

                    },
                    err => {

                        errorService.handleApiError(err, "project");

                    }).finally(() => vm.loading = false);

            }
        };

        function del() {

            if (confirm("Confirm delete?")) {

                vm.loading = true;

                projectResource.delete(
                    {
                        projectId: $stateParams.projectId
                    },
                    () => {

                        notifications.success("The project has been deleted.", "Deleted");
                        $state.go("app.projects");

                    }, err => {

                        errorService.handleApiError(err, "project", "delete");

                    })
                    .$promise.finally(() => vm.loading = false);

            }
        }

        function loadLookups(pageIndex) {

            vm.loading = true;

            var promise = lookupResource.query(
                {
                    projectId: $stateParams.projectId,
                    pageIndex: pageIndex,
                    pageSize: 0
                },
                (data, headers) => {
                    vm.lookupsHeaders = JSON.parse(headers("X-Pagination"))
                    vm.lookups = data;
                },
                err => {

                    notifications.error("Failed to load the lookups.", "Error", err);
                    $state.go("app.projects");

                }).$promise;

            promise.finally(() => vm.loading = false);

            return promise;
        }

        function loadEntities(pageIndex) {

            vm.loading = true;

            var promise = entityResource.query(
                {
                    projectId: $stateParams.projectId,
                    pageIndex: pageIndex,
                    pageSize: 0
                },
                (data, headers) => {
                    vm.entitiesHeaders = JSON.parse(headers("X-Pagination"))
                    vm.entities = data;
                    angular.forEach(vm.entities, entity => {
                        vm.multideploy[entity.entityId] = {};
                    });
                },
                err => {

                    notifications.error("Failed to load the entities.", "Error", err);
                    $state.go("app.projects");

                }).$promise;

            promise.finally(() => vm.loading = false);

            return promise;
        }

        function runMultiDeploy() {

            var data = [];
            vm.deploymentResults = null;

            angular.forEach(vm.entities, entity => {

                var item = {
                    entityId: entity.entityId,
                    model: !!vm.multideploy[entity.entityId].model,
                    enums: !!vm.multideploy[entity.entityId].enums,
                    dto: !!vm.multideploy[entity.entityId].dto,
                    settingsDTO: !!vm.multideploy[entity.entityId].settingsDTO,
                    dbContext: !!vm.multideploy[entity.entityId].dbContext,
                    controller: !!vm.multideploy[entity.entityId].controller,
                    bundleConfig: !!vm.multideploy[entity.entityId].bundleConfig,
                    appRouter: !!vm.multideploy[entity.entityId].appRouter,
                    apiResource: !!vm.multideploy[entity.entityId].apiResource,
                    listHtml: !!vm.multideploy[entity.entityId].listHtml,
                    listTypeScript: !!vm.multideploy[entity.entityId].listTypeScript,
                    editHtml: !!vm.multideploy[entity.entityId].editHtml,
                    editTypeScript: !!vm.multideploy[entity.entityId].editTypeScript
                }
                data.push(item);

            });

            vm.loading = true;

            utilitiesResource.multiDeploy(
                data,
                data => {

                    if (data.length === 0) {
                        vm.deploymentResults = null;
                        notifications.warning("Nothing was selected to deploy.", "Code Deployment");
                    } else {
                        vm.deploymentResults = data;
                        var hasErrors = false;
                        angular.forEach(vm.deploymentResults, result => {
                            if (result.isError) hasErrors = true;
                        });
                        if (hasErrors)
                            notifications.warning("Deployment has completed - see Deployment Results.", "Code Deployment");
                        else
                            notifications.success("Deployment has completed successfully.", "Code Deployment");
                    }
                },
                err => {

                    errorService.handleApiError(err, "code", "deploy");

                }).$promise.finally(() => vm.loading = false);
        }

        function setCheckBoxes(item) {
            var checked = !vm.multideploy[vm.entities[0].entityId][item];
            angular.forEach(vm.entities, entity => {
                if (item === "model" && entity.preventModelDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "dto" && entity.preventDTODeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "dbContext" && entity.preventDbContextDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "controller" && entity.preventControllerDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "bundleConfig" && entity.preventBundleConfigDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "appRouter" && entity.preventappRouterDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "apiResource" && entity.preventApiResourceDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "listHtml" && entity.preventListHtmlDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "listTypeScript" && entity.preventListTypeScriptDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "editHtml" && entity.preventEditHtmlDeployment) vm.multideploy[entity.entityId][item] = false;
                else if (item === "editTypeScript" && entity.preventEditTypeScriptDeployment) vm.multideploy[entity.entityId][item] = false;
                else vm.multideploy[entity.entityId][item] = checked;
            });
        }

        function selectAll() {
            angular.forEach(vm.entities, entity => {
                setCheckBoxesByRow(entity, true);
            });
        }

        function setCheckBoxesByRow(entity, force:any) {
            var checked = !vm.multideploy[entity.entityId]["model"];
            if (force !== undefined) checked = force;
            if (!checked || !entity.preventModelDeployment) vm.multideploy[entity.entityId]["model"] = checked;
            vm.multideploy[entity.entityId]["enums"] = checked;
            if (!checked || !entity.preventDTODeployment) vm.multideploy[entity.entityId]["dto"] = checked;
            vm.multideploy[entity.entityId]["settingsDTO"] = checked;
            if (!checked || !entity.preventDbContextDeployment) vm.multideploy[entity.entityId]["dbContext"] = checked;
            if (!checked || !entity.preventControllerDeployment) vm.multideploy[entity.entityId]["controller"] = checked;
            if (!checked || !entity.preventBundleConfigDeployment) vm.multideploy[entity.entityId]["bundleConfig"] = checked;
            if (!checked || !entity.preventappRouterDeployment) vm.multideploy[entity.entityId]["appRouter"] = checked;
            if (!checked || !entity.preventApiResourceDeployment) vm.multideploy[entity.entityId]["apiResource"] = checked;
            if (!checked || !entity.preventListHtmlDeployment) vm.multideploy[entity.entityId]["listHtml"] = checked;
            if (!checked || !entity.preventListTypeScriptDeployment) vm.multideploy[entity.entityId]["listTypeScript"] = checked;
            if (!checked || !entity.preventEditHtmlDeployment) vm.multideploy[entity.entityId]["editHtml"] = checked;
            if (!checked || !entity.preventEditTypeScriptDeployment) vm.multideploy[entity.entityId]["editTypeScript"] = checked;
        }
    };

}());
