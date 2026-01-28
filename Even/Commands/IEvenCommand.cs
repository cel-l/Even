using System.Collections.Generic;
namespace Even.Commands;

public interface IEvenCommand
{
    Command Create();
}