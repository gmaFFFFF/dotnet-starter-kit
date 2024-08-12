namespace FluentResults.Тесты;

public class РезультатРасширенияТесты {
    private readonly Result<bool> _ошибка = Result.Fail<bool>("ошибка");
    private readonly Result<bool> _ok = Result.Ok(true);

    [Fact]
    public void ЕслиОшибка() {
        _ok.Или(() => false).Should().BeTrue();
        _ошибка.Или(() => true).Should().BeTrue();

        bool? test1 = null;
        _ok.Или(Действие);
        test1.Should().BeNull();
        _ошибка.Или(Действие);
        test1.Should().BeTrue();

        void Действие() {
            test1 = true;
        }
    }

    [Fact]
    public void Если() {
        _ok.Если(a => a, () => false).Should().BeTrue();
        _ошибка.Если(_ => true, () => false).Should().BeFalse();

        bool? test1 = null;
        _ok.Если(ДействиеTrue, ДействиеFalse);
        test1.Should().BeTrue();
        test1 = null;
        _ошибка.Если(ДействиеTrue, ДействиеFalse);
        test1.Should().BeFalse();

        void ДействиеTrue(bool a) {
            test1 = a;
        }

        void ДействиеFalse() {
            test1 = false;
        }
    }
}