function GetQuantity(productID, elemToUpdate, productLabelElem, buttonElem) {
   var userContext = [productID, elemToUpdate, productLabelElem, buttonElem];
   Samples.ProductQueryService.GetProductQuantity(productID, OnSucceeded, null, userContext, null);
   $get(buttonElem).value = "Retrieving value...";
}
function OnSucceeded(result, userContext) {
   var productID = userContext[0];
   var elemToUpdate = userContext[1];
   var productLabelElem = userContext[2];
   var buttonElem = userContext[3];
   $get(buttonElem).value = "Get Quantity from Web Service";
   if ($get(elemToUpdate) !== null && $get(productLabelElem).innerHTML == productID) {
     $get(elemToUpdate).value = result;
   }
}
