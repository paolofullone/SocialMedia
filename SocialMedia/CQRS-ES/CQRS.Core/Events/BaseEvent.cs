using CQRS.Core.Messages;

namespace CQRS.Core.Events
{
    public abstract class BaseEvent : Message
    {
        protected BaseEvent(string type)
        {
            this.Type = type;
        }

        public int Version { get; set; }
        public string Type { get; set; }
    }
}

// we are using polymorphism, since all our concrete events extends the base event classe.
// I got an error while trying to test the PUT method, sayin we cannot instantiate an abstract class.
// MongoDB in it's default state does not support polymorphism, so we set the BsonClassmethod in Program.CS