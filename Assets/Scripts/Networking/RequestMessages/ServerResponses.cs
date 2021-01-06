namespace Networking.RequestMessages {
    public struct ServerResponses {
        public const string Success = "SUCCESS";
        public const string RgUsernameExist = "ERROR: USERNAME EXISTS";
        public const string LgError = "ERROR: USERNAME OR PASSWORD INCORRECT";
        public const string InvalidUsernamePassword = "ERROR: INVALID USERNAME OR PASSWORD";
        public const string UnexpectedError = "UNEXPECTED ERROR";
        public const string TryBuyError = "ERROR: USERNAME DOES NOT EXIST OR NOT ENOUGH BALANCE";
        public const string UsernameNotExist = "ERROR: USERNAME DOES NOT EXIST";
        public const string SessionUpdateError = "ERROR: SESSION TOKEN COULD NOT BE UPDATED";
        public const string SessionTimeInvalid = "ERROR: SESSION TIME IS INVALID";
    }
}