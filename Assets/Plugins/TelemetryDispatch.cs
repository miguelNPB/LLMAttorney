using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fachada de telemetría para gameplay.
/// Unifica el envío de eventos al EventManager y evita duplicar lógica de contexto en cada callsite.
/// </summary>
namespace Telemetry
{
    public static class TelemetryDispatch
    {

        public static void SendNotConsistentAnswer(int messageID)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.SendNotConsistentAnswerEvent(messageID, phaseID);
        }

        public static void SendDeniedBudget(int messageID, int price)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.SendDeniedBudgetEvent(messageID, price);
        }

        public static void SendPostDocument(int documentType)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.SendPostDocumentEvent(documentType);
        }

        public static void SendReceivedDocument(int documentType, bool isValid)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.SendReceivedDocumentEvent(documentType, isValid);
        }

        public static void SendAskedDocument(int price, int documentType)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.SendAskedDocumentEvent(price, documentType);
        }

        public static void SendQueryPost(int messageID)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.sendQueryPostEvent(messageID, phaseID);
        }

        public static void SendQueryReceived(int messageID)
        {
            if (!TryGetContext(out EventManager manager, out int phaseID)) return;

            manager.sendQueryReceivedEvent(messageID, phaseID);
        }

        // Resuelve el contexto mínimo necesario para enviar telemetría.
        private static bool TryGetContext(out EventManager manager, out int phaseID)
        {
            manager = UnityEngine.Object.FindAnyObjectByType<EventManager>();

            if (manager == null)
            {
                GameObject eventManagerObject = new GameObject("EventManager_Auto");
                manager = eventManagerObject.AddComponent<EventManager>();
            }

            if (manager == null)
            {
                phaseID = -1;
                return false;
            }

            phaseID = SceneManager.GetActiveScene().buildIndex;       
            
            return true;
        }
    }

}
