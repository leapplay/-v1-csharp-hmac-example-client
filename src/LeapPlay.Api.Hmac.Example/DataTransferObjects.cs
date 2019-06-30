using System.ComponentModel.DataAnnotations;

namespace LeapPlay.Api.Hmac.Example
{
    /// <summary>
    /// Request Model to change the display settings of an Station
    /// </summary>
    public class RequestStationSettingsDto
    {
        /// <summary>
        /// The name displayed by the station and returned by the API
        /// </summary>
        [Required, MinLength(3), MaxLength(15)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The operational mode of the station
        /// </summary>
        [Required]
        public StationControlMode Mode { get; set; }

        /// <summary>
        /// A String as QR Code, must be set when <see cref="StationControlMode.RemoteWithQrCode"/> is used
        /// </summary>
        [MinLength(1), MaxLength(128)]
        public string QrCode { get; set; }
    }

    public enum StationControlMode
    {
        /// <summary>
        /// Invalid Value
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// The Station can be operated locally and remotely, local settings are unlocked for every user
        /// </summary>
        Local,
        /// <summary>
        /// The Station can only be operated remotely, local settings are protected by a PIN
        /// </summary>
        Remote,
        /// <summary>
        /// The Station shows a QRCode and can only be operated remotely, local settings are protected by a PIN
        /// </summary>
        RemoteWithQrCode
    }
}
