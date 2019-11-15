module TodoFs.Api.HttpResponses

open Microsoft.AspNetCore.Mvc

let httpOk() =
    OkResult() :> IActionResult

let httpOkObject x =
    OkObjectResult x :> IActionResult

let httpCreatedAt actionName routeValues value =
    CreatedAtActionResult(actionName, null, routeValues, value) :> IActionResult

let httpNotFound() =
    NotFoundResult() :> IActionResult

let httpNoContent() =
    NoContentResult() :> IActionResult

let httpBadRequest() =
    BadRequestResult() :> IActionResult

let httpBadRequestObject (error: obj) =
    BadRequestObjectResult error :> IActionResult
