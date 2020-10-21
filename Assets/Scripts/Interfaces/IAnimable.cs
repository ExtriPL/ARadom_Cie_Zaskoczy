using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAnimable
{
    void OnCloseAnimationStart();

    void OnShowAnimationStart();

    void OnCloseAnimationEnd();

    void OnShowAnimationEnd();
}